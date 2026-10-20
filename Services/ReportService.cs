using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reports;
using soromaps_api.Errors;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pela gestão de denúncias de conteúdo e fila de triagem administrativa.
 */
public sealed class ReportService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IReportService
{
    /**
     * Registra denúncia de usuário contra um local, avaliação ou comentário impedindo auto-denúncias e duplicatas.
     */
    public async Task<Result> CreateReportAsync(CreateReportDto request, CancellationToken cancellationToken = default)
    {
        var reporterId = currentUser.UserId;

        Guid? authorId;
        switch (request.TargetType)
        {
            case ReportTargetType.Place:
                var place = await context.Places.AsNoTracking().SingleOrDefaultAsync(p => p.Id == request.TargetId, cancellationToken);
                if (place is null)
                    return ReportErrors.TargetNotFound;
                authorId = place.AuthorId;
                break;

            case ReportTargetType.Review:
                var review = await context.Reviews.AsNoTracking().SingleOrDefaultAsync(r => r.Id == request.TargetId && r.RemovedAt == null, cancellationToken);
                if (review is null)
                    return ReportErrors.TargetNotFound;
                authorId = review.UserId;
                break;

            case ReportTargetType.Comment:
                var comment = await context.Comments.AsNoTracking().SingleOrDefaultAsync(c => c.Id == request.TargetId && c.RemovedAt == null, cancellationToken);
                if (comment is null)
                    return ReportErrors.TargetNotFound;
                authorId = comment.UserId;
                break;

            default:
                return ReportErrors.InvalidTargetType;
        }

        if (authorId.HasValue && authorId.Value == reporterId)
            return ReportErrors.CannotReportOwn;

        var alreadyReported = await context.Reports
            .AnyAsync(r => r.ReporterId == reporterId && r.TargetType == request.TargetType && r.TargetId == request.TargetId, cancellationToken);

        if (alreadyReported)
            return ReportErrors.AlreadyReported;

        var report = new Report
        {
            ReporterId = reporterId,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            Reason = request.Reason.Trim(),
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = ReportStatus.Open,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        context.Reports.Add(report);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /**
     * Lista denúncias abertas agrupadas por conteúdo alvo ordenadas pelo total de denunciantes distintos.
     */
    public async Task<Result<PagedResponseDto<AdminGroupedReportItemDto>>> ListAdminGroupedReportsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var openReports = await context.Reports
            .AsNoTracking()
            .Where(r => r.Status == ReportStatus.Open)
            .GroupBy(r => new { r.TargetType, r.TargetId })
            .Select(g => new
            {
                g.Key.TargetType,
                g.Key.TargetId,
                ReportersCount = g.Select(r => r.ReporterId).Distinct().Count(),
                Reasons = g.Select(r => r.Reason).Distinct().ToList(),
                FirstReportedAt = g.Min(r => r.CreatedAt),
                LastReportedAt = g.Max(r => r.CreatedAt)
            })
            .OrderByDescending(x => x.ReportersCount)
            .ThenByDescending(x => x.LastReportedAt)
            .ToListAsync(cancellationToken);

        var totalCount = openReports.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);
        var pagedGrouped = openReports
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var resultItems = new List<AdminGroupedReportItemDto>();

        foreach (var item in pagedGrouped)
        {
            var snapshot = await BuildTargetSnapshotAsync(item.TargetType, item.TargetId, cancellationToken);
            if (snapshot is not null)
            {
                resultItems.Add(new AdminGroupedReportItemDto(
                    item.TargetType,
                    item.TargetId,
                    item.ReportersCount,
                    item.Reasons,
                    snapshot,
                    item.FirstReportedAt,
                    item.LastReportedAt
                ));
            }
        }

        return new PagedResponseDto<AdminGroupedReportItemDto>(
            resultItems,
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages
        );
    }

    /**
     * Descarta denúncias de um alvo mantendo o conteúdo ativo e marcando as denúncias como resolvidas.
     */
    public async Task<Result> DismissReportsAsync(
        string targetType,
        Guid targetId,
        DismissReportsDto request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<ReportTargetType>(targetType, true, out var parsedTargetType))
            return ReportErrors.InvalidTargetType;

        var reports = await context.Reports
            .Where(r => r.TargetType == parsedTargetType && r.TargetId == targetId && r.Status == ReportStatus.Open)
            .ToListAsync(cancellationToken);

        if (reports.Count == 0)
            return ReportErrors.TargetHasNoOpenReports;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var moderatorId = currentUser.UserId;

        foreach (var report in reports)
        {
            report.Status = ReportStatus.ResolvedKept;
            report.ResolvedById = moderatorId;
            report.ResolutionNote = string.IsNullOrWhiteSpace(request.ResolutionNote) ? null : request.ResolutionNote.Trim();
            report.ResolvedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<ReportTargetSnapshotDto?> BuildTargetSnapshotAsync(
        ReportTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken)
    {
        switch (targetType)
        {
            case ReportTargetType.Place:
                var place = await context.Places
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .SingleOrDefaultAsync(p => p.Id == targetId, cancellationToken);

                if (place is null)
                    return null;

                return new ReportTargetSnapshotDto(
                    place.Id,
                    ReportTargetType.Place,
                    place.Name,
                    place.Neighborhood,
                    place.About,
                    place.Author == null ? null : new AuthorSummaryDto(place.Author.Id, place.Author.Name, place.Author.AvatarUrl),
                    place.CreatedAt
                );

            case ReportTargetType.Review:
                var review = await context.Reviews
                    .AsNoTracking()
                    .Include(r => r.Place)
                    .Include(r => r.User)
                    .SingleOrDefaultAsync(r => r.Id == targetId, cancellationToken);

                if (review is null)
                    return null;

                return new ReportTargetSnapshotDto(
                    review.Id,
                    ReportTargetType.Review,
                    $"Avaliação em {review.Place.Name}",
                    $"Nota: {review.Rating}/5",
                    review.Content,
                    review.User == null ? null : new AuthorSummaryDto(review.User.Id, review.User.Name, review.User.AvatarUrl),
                    review.CreatedAt
                );

            case ReportTargetType.Comment:
                var comment = await context.Comments
                    .AsNoTracking()
                    .Include(c => c.Review).ThenInclude(r => r.Place)
                    .Include(c => c.User)
                    .SingleOrDefaultAsync(c => c.Id == targetId, cancellationToken);

                if (comment is null)
                    return null;

                return new ReportTargetSnapshotDto(
                    comment.Id,
                    ReportTargetType.Comment,
                    $"Comentário em avaliação de {comment.Review.Place.Name}",
                    null,
                    comment.Content,
                    comment.User == null ? null : new AuthorSummaryDto(comment.User.Id, comment.User.Name, comment.User.AvatarUrl),
                    comment.CreatedAt
                );

            default:
                return null;
        }
    }
}
