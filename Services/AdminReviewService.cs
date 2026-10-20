using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Admin;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Models;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço de auditoria administrativa de avaliações e remoção de conteúdo prejudicial.
 * Oferece detecção de anomalias estatísticas em avaliações e rastreabilidade com justificativa formal.
 */
public sealed class AdminReviewService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IAdminReviewService
{
    private const double OutlierVarianceThreshold = 2.0;
    private const int OutlierMinimumReviewsThreshold = 5;
    private const int BurstReviewsThreshold = 3;
    private static readonly TimeSpan BurstWindow = TimeSpan.FromHours(24);

    /**
     * Consulta avaliações ativas aplicando regras heurísticas de sinais de alerta (outlier de nota e rajada).
     */
    public async Task<Result<PagedResponseDto<AdminReviewItemDto>>> ListReviewsAsync(
        PaginationQueryDto pagination,
        AdminReviewQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var reviewsQuery = context.Reviews
            .AsNoTracking()
            .Include(r => r.Place)
            .Include(r => r.User)
            .Include(r => r.UsefulVotes)
            .Where(r => r.RemovedAt == null);

        if (query.Rating.HasValue)
            reviewsQuery = reviewsQuery.Where(r => r.Rating == query.Rating.Value);

        if (query.PlaceId.HasValue)
            reviewsQuery = reviewsQuery.Where(r => r.PlaceId == query.PlaceId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            reviewsQuery = reviewsQuery.Where(r =>
                EF.Functions.ILike(r.Content, pattern) ||
                EF.Functions.ILike(r.Place.Name, pattern));
        }

        var placeIds = await reviewsQuery
            .Select(r => r.PlaceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var placeAverages = await context.Reviews
            .AsNoTracking()
            .Where(r => placeIds.Contains(r.PlaceId) && r.RemovedAt == null)
            .GroupBy(r => r.PlaceId)
            .Select(g => new
            {
                PlaceId = g.Key,
                Average = g.Average(r => (double)r.Rating),
                Total = g.Count()
            })
            .ToDictionaryAsync(item => item.PlaceId, cancellationToken);

        var reviews = await reviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var authorIds = reviews
            .Where(r => r.UserId.HasValue)
            .Select(r => r.UserId!.Value)
            .Distinct()
            .ToList();

        var authorReviewsInLast24h = await context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId.HasValue && authorIds.Contains(r.UserId.Value) && r.CreatedAt >= timeProvider.GetUtcNow().UtcDateTime - BurstWindow)
            .GroupBy(r => r.UserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);

        var items = reviews.Select(r =>
        {
            var signals = new List<string>();

            if (placeAverages.TryGetValue(r.PlaceId, out var stats) && stats.Total >= OutlierMinimumReviewsThreshold)
            {
                if (Math.Abs(r.Rating - stats.Average) >= OutlierVarianceThreshold)
                    signals.Add("outlier");
            }

            if (r.UserId.HasValue && authorReviewsInLast24h.TryGetValue(r.UserId.Value, out var burstCount) && burstCount >= BurstReviewsThreshold)
                signals.Add("burst");

            var placeAvg = stats?.Average ?? (double)r.Rating;

            return new AdminReviewItemDto(
                r.Id,
                r.PlaceId,
                r.Place.Name,
                r.User == null ? null : new AuthorSummaryDto(r.User.Id, r.User.Name, r.User.AvatarUrl),
                r.Content,
                r.Rating,
                Math.Round(placeAvg, 1),
                r.UsefulVotes.Count,
                signals,
                r.CreatedAt
            );
        }).ToList();

        if (!string.IsNullOrWhiteSpace(query.Signal))
        {
            var requestedSignal = query.Signal.Trim().ToLowerInvariant();
            if (requestedSignal is "outlier" or "burst")
                items = items.Where(item => item.Signals.Contains(requestedSignal)).ToList();
        }

        var totalCount = items.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);
        var pagedItems = items
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return new PagedResponseDto<AdminReviewItemDto>(
            pagedItems,
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages
        );
    }

    /**
     * Aplica a remoção lógica do conteúdo alvo (avaliação ou comentário) e registra a decisão na tabela de remoções.
     */
    public async Task<Result<ContentRemovalResponseDto>> RemoveContentAsync(
        ContentRemovalRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var targetType = request.TargetType.Trim().ToLowerInvariant();
        if (targetType is not ("review" or "comment"))
            return RemovalErrors.InvalidTargetType;

        var moderatorId = currentUser.UserId;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        Guid? authorId;
        string snapshotText;

        if (targetType == "review")
        {
            var review = await context.Reviews
                .SingleOrDefaultAsync(r => r.Id == request.TargetId, cancellationToken);

            if (review is null || review.RemovedAt != null)
                return RemovalErrors.TargetNotFound;

            review.RemovedAt = now;
            authorId = review.UserId;
            snapshotText = review.Content;
        }
        else
        {
            var comment = await context.Comments
                .SingleOrDefaultAsync(c => c.Id == request.TargetId, cancellationToken);

            if (comment is null || comment.RemovedAt != null)
                return RemovalErrors.TargetNotFound;

            comment.RemovedAt = now;
            authorId = comment.UserId;
            snapshotText = comment.Content;
        }

        var removal = new ContentRemoval
        {
            TargetType = targetType,
            TargetId = request.TargetId,
            AuthorId = authorId,
            ModeratorId = moderatorId,
            Reason = request.Reason.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SnapshotText = snapshotText,
            CreatedAt = now
        };

        context.ContentRemovals.Add(removal);
        await context.SaveChangesAsync(cancellationToken);

        var author = authorId.HasValue
            ? await context.Users.AsNoTracking().Where(u => u.Id == authorId.Value).Select(u => new AuthorSummaryDto(u.Id, u.Name, u.AvatarUrl)).SingleOrDefaultAsync(cancellationToken)
            : null;

        var moderator = await context.Users.AsNoTracking().Where(u => u.Id == moderatorId).Select(u => new AuthorSummaryDto(u.Id, u.Name, u.AvatarUrl)).SingleOrDefaultAsync(cancellationToken);

        return new ContentRemovalResponseDto(
            removal.Id,
            removal.TargetType,
            removal.TargetId,
            author,
            moderator,
            removal.Reason,
            removal.Note,
            removal.SnapshotText,
            removal.CreatedAt
        );
    }

    /**
     * Consulta o histórico paginado de ações moderatórias de remoção realizadas na plataforma.
     */
    public async Task<Result<PagedResponseDto<ContentRemovalResponseDto>>> ListRemovalsHistoryAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var query = context.ContentRemovals
            .AsNoTracking()
            .Include(cr => cr.Author)
            .Include(cr => cr.Moderator)
            .OrderByDescending(cr => cr.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(cr => new ContentRemovalResponseDto(
                cr.Id,
                cr.TargetType,
                cr.TargetId,
                cr.Author == null ? null : new AuthorSummaryDto(cr.Author.Id, cr.Author.Name, cr.Author.AvatarUrl),
                cr.Moderator == null ? null : new AuthorSummaryDto(cr.Moderator.Id, cr.Moderator.Name, cr.Moderator.AvatarUrl),
                cr.Reason,
                cr.Note,
                cr.SnapshotText,
                cr.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new PagedResponseDto<ContentRemovalResponseDto>(
            items,
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages
        );
    }
}
