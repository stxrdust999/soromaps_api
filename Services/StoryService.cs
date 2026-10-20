using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Stories;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pela gestão de pautas editoriais e curadorias de locais.
 */
public sealed class StoryService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IStoryService
{
    /**
     * Lista pautas editoriais de forma paginada respeitando a política de visualização por perfil.
     */
    public async Task<Result<PagedResponseDto<StorySummaryDto>>> ListAsync(
        PaginationQueryDto pagination,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Stories
            .AsNoTracking()
            .Include(s => s.Author)
            .Include(s => s.StoryPlaces).ThenInclude(sp => sp.Place).ThenInclude(p => p.Photos)
            .AsQueryable();

        var isAdmin = currentUser.IsAuthenticated && currentUser.IsInRole(UserRole.Admin);

        if (!isAdmin)
        {
            query = query.Where(s => s.Status == StoryStatus.Published);
        }
        else if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StoryStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(s => s.Status == parsedStatus);
        }

        query = query.OrderByDescending(s => s.PublishedAt ?? s.CreatedAt);

        return await query
            .Select(s => new StorySummaryDto(
                s.Id,
                s.Slug,
                s.Title,
                s.Subtitle,
                s.Neighborhood,
                s.Origin,
                s.Status,
                new AuthorSummaryDto(s.Author.Id, s.Author.Name, s.Author.AvatarUrl),
                s.StoryPlaces.Count,
                s.StoryPlaces
                    .OrderBy(sp => sp.SortOrder)
                    .Select(sp => sp.Place.Photos.OrderByDescending(p => p.IsCover).Select(p => p.Url).FirstOrDefault())
                    .FirstOrDefault(),
                s.PublishedAt,
                s.CreatedAt
            ))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Busca detalhes da pauta através do slug, restringindo rascunhos para administradores.
     */
    public async Task<Result<StoryDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        var story = await context.Stories
            .AsNoTracking()
            .Include(s => s.Author)
            .Include(s => s.StoryPlaces).ThenInclude(sp => sp.Place).ThenInclude(p => p.Category)
            .Include(s => s.StoryPlaces).ThenInclude(sp => sp.Place).ThenInclude(p => p.Photos)
            .SingleOrDefaultAsync(s => s.Slug == normalizedSlug, cancellationToken);

        if (story is null)
            return StoryErrors.NotFound;

        var isAdmin = currentUser.IsAuthenticated && currentUser.IsInRole(UserRole.Admin);
        if (story.Status == StoryStatus.Draft && !isAdmin)
            return StoryErrors.NotFound;

        var places = story.StoryPlaces
            .OrderBy(sp => sp.SortOrder)
            .Select(sp => new StoryPlaceItemDto(
                sp.PlaceId,
                sp.Place.Name,
                sp.Place.Neighborhood,
                new CategorySummaryDto(sp.Place.Category.Id, sp.Place.Category.Name, sp.Place.Category.Slug, sp.Place.Category.Icon, sp.Place.Category.Color),
                sp.Place.Photos.OrderByDescending(p => p.IsCover).Select(p => p.Url).FirstOrDefault(),
                sp.SortOrder,
                sp.AngleTitle,
                sp.BodyText,
                sp.LocalTip
            ))
            .ToList();

        return new StoryDetailDto(
            story.Id,
            story.Slug,
            story.Title,
            story.Subtitle,
            story.Neighborhood,
            story.IntroText,
            story.ClosingText,
            story.Origin,
            story.Status,
            new AuthorSummaryDto(story.Author.Id, story.Author.Name, story.Author.AvatarUrl),
            places,
            story.PublishedAt,
            story.CreatedAt,
            story.UpdatedAt
        );
    }

    /**
     * Cria um novo rascunho de pauta editorial validando unicidade de slug.
     */
    public async Task<Result<StoryDetailDto>> CreateAsync(CreateStoryDto request, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var slugExists = await context.Stories.AnyAsync(s => s.Slug == normalizedSlug, cancellationToken);
        if (slugExists)
            return StoryErrors.SlugTaken;

        var authorId = currentUser.UserId;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var story = new Story
        {
            Slug = normalizedSlug,
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle.Trim(),
            Neighborhood = string.IsNullOrWhiteSpace(request.Neighborhood) ? null : request.Neighborhood.Trim(),
            IntroText = request.IntroText.Trim(),
            ClosingText = string.IsNullOrWhiteSpace(request.ClosingText) ? null : request.ClosingText.Trim(),
            Origin = request.Origin,
            Status = StoryStatus.Draft,
            AuthorId = authorId,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (request.Places is { Count: > 0 })
        {
            foreach (var placeInput in request.Places)
            {
                story.StoryPlaces.Add(new StoryPlace
                {
                    PlaceId = placeInput.PlaceId,
                    SortOrder = placeInput.SortOrder,
                    AngleTitle = placeInput.AngleTitle.Trim(),
                    BodyText = placeInput.BodyText.Trim(),
                    LocalTip = string.IsNullOrWhiteSpace(placeInput.LocalTip) ? null : placeInput.LocalTip.Trim()
                });
            }
        }

        context.Stories.Add(story);
        await context.SaveChangesAsync(cancellationToken);

        return await GetBySlugAsync(story.Slug, cancellationToken);
    }

    /**
     * Atualiza dados de conteúdo e a lista de locais de uma pauta editorial.
     */
    public async Task<Result<StoryDetailDto>> UpdateAsync(Guid id, UpdateStoryDto request, CancellationToken cancellationToken = default)
    {
        var story = await context.Stories
            .Include(s => s.StoryPlaces)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (story is null)
            return StoryErrors.NotFound;

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
            if (normalizedSlug != story.Slug)
            {
                var slugExists = await context.Stories.AnyAsync(s => s.Id != id && s.Slug == normalizedSlug, cancellationToken);
                if (slugExists)
                    return StoryErrors.SlugTaken;

                story.Slug = normalizedSlug;
            }
        }

        if (request.Title is not null)
            story.Title = request.Title.Trim();

        if (request.Subtitle is not null)
            story.Subtitle = request.Subtitle.Trim();

        if (request.Neighborhood is not null)
            story.Neighborhood = string.IsNullOrWhiteSpace(request.Neighborhood) ? null : request.Neighborhood.Trim();

        if (request.IntroText is not null)
            story.IntroText = request.IntroText.Trim();

        if (request.ClosingText is not null)
            story.ClosingText = string.IsNullOrWhiteSpace(request.ClosingText) ? null : request.ClosingText.Trim();

        if (request.Places is not null)
        {
            context.StoryPlaces.RemoveRange(story.StoryPlaces);
            story.StoryPlaces.Clear();

            foreach (var placeInput in request.Places)
            {
                story.StoryPlaces.Add(new StoryPlace
                {
                    StoryId = story.Id,
                    PlaceId = placeInput.PlaceId,
                    SortOrder = placeInput.SortOrder,
                    AngleTitle = placeInput.AngleTitle.Trim(),
                    BodyText = placeInput.BodyText.Trim(),
                    LocalTip = string.IsNullOrWhiteSpace(placeInput.LocalTip) ? null : placeInput.LocalTip.Trim()
                });
            }
        }

        story.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await context.SaveChangesAsync(cancellationToken);

        return await GetBySlugAsync(story.Slug, cancellationToken);
    }

    /**
     * Publica a pauta editorial após validar que todos os locais associados estão aprovados na plataforma.
     */
    public async Task<Result<StoryDetailDto>> PublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var story = await context.Stories
            .Include(s => s.StoryPlaces)
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (story is null)
            return StoryErrors.NotFound;

        if (story.Status == StoryStatus.Published)
            return StoryErrors.AlreadyPublished;

        if (story.StoryPlaces.Count == 0)
            return StoryErrors.NoPlacesLinked;

        var placeIds = story.StoryPlaces.Select(sp => sp.PlaceId).ToList();
        var unapprovedPlacesExist = await context.Places
            .AnyAsync(p => placeIds.Contains(p.Id) && p.Status != PlaceStatus.Approved, cancellationToken);

        if (unapprovedPlacesExist)
            return StoryErrors.UnapprovedPlacesLinked;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        story.Status = StoryStatus.Published;
        story.PublishedAt = now;
        story.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        return await GetBySlugAsync(story.Slug, cancellationToken);
    }
}
