using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feed;
using soromaps_api.Errors;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por agregar eventos de locais no feed dinâmico e gerenciar regras de silenciamento.
 */
public sealed class FeedService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IFeedService
{
    /**
     * Monta o feed do usuário agregando novas avaliações, novos locais, rajadas de visitas e pautas publicadas.
     */
    public async Task<Result<PagedResponseDto<FeedItemDto>>> GetFeedAsync(FeedQueryDto query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var mutes = await context.FeedMutes
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

        var mutedNeighborhoods = mutes
            .Where(m => m.ScopeType == FeedMuteScopeType.Neighborhood)
            .Select(m => m.ScopeValue.ToLowerInvariant())
            .ToHashSet();

        var mutedCategories = mutes
            .Where(m => m.ScopeType == FeedMuteScopeType.Category)
            .Select(m => m.ScopeValue.ToLowerInvariant())
            .ToHashSet();

        var mutedKinds = mutes
            .Where(m => m.ScopeType == FeedMuteScopeType.ItemKind)
            .Select(m => m.ScopeValue.ToLowerInvariant())
            .ToHashSet();

        var favoritePlaceIds = await context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.PlaceId)
            .ToListAsync(cancellationToken);

        var favoritePlaceSet = favoritePlaceIds.ToHashSet();

        var feedItems = new List<FeedItemDto>();

        // 1. Novas avaliações recentes
        if (!mutedKinds.Contains("review"))
        {
            var reviews = await context.Reviews
                .AsNoTracking()
                .Include(r => r.Place).ThenInclude(p => p.Category)
                .Include(r => r.User)
                .Where(r => r.RemovedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .Take(40)
                .ToListAsync(cancellationToken);

            foreach (var r in reviews)
            {
                if (mutedNeighborhoods.Contains(r.Place.Neighborhood.ToLowerInvariant()))
                    continue;

                if (mutedCategories.Contains(r.Place.Category.Slug.ToLowerInvariant()))
                    continue;

                var reason = favoritePlaceSet.Contains(r.PlaceId) ? "salvo" : "cidade";

                feedItems.Add(new FeedItemDto(
                    $"review_{r.Id}",
                    "review",
                    reason,
                    $"{r.User?.Name ?? "Um explorador"} avaliou {r.Place.Name}",
                    r.Content,
                    r.Place.Neighborhood,
                    r.Place.Id,
                    r.Place.Name,
                    r.Place.Category.Name,
                    r.Place.Category.Color,
                    r.Place.Category.Icon,
                    null,
                    new { r.Rating, r.UsefulVotes.Count },
                    r.CreatedAt
                ));
            }
        }

        // 2. Novos locais aprovados
        if (!mutedKinds.Contains("new_place"))
        {
            var places = await context.Places
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.Status == PlaceStatus.Approved)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var p in places)
            {
                if (mutedNeighborhoods.Contains(p.Neighborhood.ToLowerInvariant()))
                    continue;

                if (mutedCategories.Contains(p.Category.Slug.ToLowerInvariant()))
                    continue;

                var coverPhoto = p.Photos.OrderByDescending(photo => photo.IsCover).Select(photo => photo.Url).FirstOrDefault();

                feedItems.Add(new FeedItemDto(
                    $"place_{p.Id}",
                    "new_place",
                    "cidade",
                    $"Novo local aprovado: {p.Name}",
                    p.About,
                    p.Neighborhood,
                    p.Id,
                    p.Name,
                    p.Category.Name,
                    p.Category.Color,
                    p.Category.Icon,
                    coverPhoto,
                    new { p.HasWifi, p.IsPetFriendly },
                    p.CreatedAt
                ));
            }
        }

        // 3. Pautas editoriais publicadas
        if (!mutedKinds.Contains("story"))
        {
            var stories = await context.Stories
                .AsNoTracking()
                .Include(s => s.StoryPlaces).ThenInclude(sp => sp.Place).ThenInclude(p => p.Photos)
                .Where(s => s.Status == StoryStatus.Published)
                .OrderByDescending(s => s.PublishedAt ?? s.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var s in stories)
            {
                if (s.Neighborhood is not null && mutedNeighborhoods.Contains(s.Neighborhood.ToLowerInvariant()))
                    continue;

                var coverPhoto = s.StoryPlaces
                    .OrderBy(sp => sp.SortOrder)
                    .Select(sp => sp.Place.Photos.OrderByDescending(p => p.IsCover).Select(p => p.Url).FirstOrDefault())
                    .FirstOrDefault();

                feedItems.Add(new FeedItemDto(
                    $"story_{s.Id}",
                    "story",
                    "curadoria",
                    s.Title,
                    s.Subtitle,
                    s.Neighborhood,
                    null,
                    null,
                    "Pauta Especial",
                    "#6366F1",
                    "newspaper",
                    coverPhoto,
                    new { s.Slug, PlacesCount = s.StoryPlaces.Count },
                    s.PublishedAt ?? s.CreatedAt
                ));
            }
        }

        // 4. Rajadas de visitas recentes (≥ 3 visitas no mesmo dia em local aprovado)
        if (!mutedKinds.Contains("place_burst"))
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            var today = DateOnly.FromDateTime(now);
            var threeDaysAgo = today.AddDays(-3);

            var bursts = await context.Visits
                .AsNoTracking()
                .Include(v => v.Place).ThenInclude(p => p.Category)
                .Where(v => v.VisitedOn >= threeDaysAgo && v.Place.Status == PlaceStatus.Approved)
                .GroupBy(v => new { v.PlaceId, v.VisitedOn, v.Place.Name, v.Place.Neighborhood, CategoryName = v.Place.Category.Name, CategoryColor = v.Place.Category.Color, CategoryIcon = v.Place.Category.Icon, CategorySlug = v.Place.Category.Slug })
                .Where(g => g.Count() >= 3)
                .Select(g => new
                {
                    g.Key.PlaceId,
                    g.Key.VisitedOn,
                    g.Key.Name,
                    g.Key.Neighborhood,
                    g.Key.CategoryName,
                    g.Key.CategoryColor,
                    g.Key.CategoryIcon,
                    g.Key.CategorySlug,
                    VisitsCount = g.Count()
                })
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var b in bursts)
            {
                if (mutedNeighborhoods.Contains(b.Neighborhood.ToLowerInvariant()))
                    continue;

                if (mutedCategories.Contains(b.CategorySlug.ToLowerInvariant()))
                    continue;

                feedItems.Add(new FeedItemDto(
                    $"burst_{b.PlaceId}_{b.VisitedOn:yyyyMMdd}",
                    "place_burst",
                    "cidade",
                    $"{b.Name} em alta hoje!",
                    $"{b.VisitsCount} exploradores fizeram check-in recentemente por lá.",
                    b.Neighborhood,
                    b.PlaceId,
                    b.Name,
                    b.CategoryName,
                    b.CategoryColor,
                    b.CategoryIcon,
                    null,
                    new { b.VisitsCount },
                    b.VisitedOn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                ));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.Reason))
        {
            var requestedReason = query.Reason.Trim().ToLowerInvariant();
            feedItems = feedItems.Where(i => i.Reason.ToLowerInvariant() == requestedReason).ToList();
        }

        if (!string.IsNullOrWhiteSpace(query.Neighborhood))
        {
            var requestedNeighborhood = query.Neighborhood.Trim().ToLowerInvariant();
            feedItems = feedItems.Where(i => i.Neighborhood?.ToLowerInvariant() == requestedNeighborhood).ToList();
        }

        feedItems = feedItems.OrderByDescending(i => i.OccurredAt).ToList();

        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 20;
        var total = feedItems.Count;
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var pagedItems = feedItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResponseDto<FeedItemDto>(
            pagedItems,
            page,
            pageSize,
            total,
            totalPages
        );
    }

    /**
     * Lista todos os filtros de silenciamento ativos configurados pelo usuário.
     */
    public async Task<Result<IReadOnlyList<FeedMuteResponseDto>>> ListMutesAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var mutes = await context.FeedMutes
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new FeedMuteResponseDto(m.Id, m.ScopeType, m.ScopeValue, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return mutes;
    }

    /**
     * Cadastra um novo silenciamento (ex: silenciar determinado bairro ou categoria).
     */
    public async Task<Result<FeedMuteResponseDto>> AddMuteAsync(CreateFeedMuteDto request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var normalizedValue = request.ScopeValue.Trim().ToLowerInvariant();

        var alreadyMuted = await context.FeedMutes
            .AnyAsync(m => m.UserId == userId && m.ScopeType == request.ScopeType && m.ScopeValue.ToLower() == normalizedValue, cancellationToken);

        if (alreadyMuted)
            return FeedMuteErrors.AlreadyMuted;

        var mute = new FeedMute
        {
            UserId = userId,
            ScopeType = request.ScopeType,
            ScopeValue = request.ScopeValue.Trim(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        context.FeedMutes.Add(mute);
        await context.SaveChangesAsync(cancellationToken);

        return new FeedMuteResponseDto(mute.Id, mute.ScopeType, mute.ScopeValue, mute.CreatedAt);
    }

    /**
     * Remove um silenciamento ativo restaurando a exibição dos itens no feed.
     */
    public async Task<Result> RemoveMuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var mute = await context.FeedMutes
            .SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);

        if (mute is null)
            return FeedMuteErrors.NotFound;

        context.FeedMutes.Remove(mute);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
