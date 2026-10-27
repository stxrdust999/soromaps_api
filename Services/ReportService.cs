using System.Globalization;
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
    private const int RequiredVisitsForVerification = 5;
    private const int RequiredReviewsForVerification = 3;
    private const int MaxAllowedRemovedReviewsForVerification = 0;
    private const int RequiredAccountAgeDaysForVerification = 30;
    private static readonly CultureInfo PtBrCulture = new("pt-BR");

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
     * Lista denúncias abertas agrupadas por conteúdo alvo ordenadas pelo total de denunciantes distintos, enriquecendo alvos, autores e denunciantes em lote.
     */
    public async Task<Result<PagedResponseDto<AdminGroupedReportItemDto>>> ListAdminGroupedReportsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        /** Carrega todas as denúncias abertas com seus denunciantes em uma única consulta para agrupamento sem N+1. */
        var allOpenReports = await context.Reports
            .AsNoTracking()
            .Include(report => report.Reporter)
            .Where(report => report.Status == ReportStatus.Open)
            .ToListAsync(cancellationToken);

        var groupedReports = allOpenReports
            .GroupBy(report => new { report.TargetType, report.TargetId })
            .Select(group => new
            {
                group.Key.TargetType,
                group.Key.TargetId,
                ReportersCount = group.Select(report => report.ReporterId).Distinct().Count(),
                Reasons = group.Select(report => report.Reason).Distinct().ToList(),
                FirstReportedAt = group.Min(report => report.CreatedAt),
                LastReportedAt = group.Max(report => report.CreatedAt),
                Reports = group.OrderByDescending(report => report.CreatedAt).ToList()
            })
            .OrderByDescending(group => group.ReportersCount)
            .ThenByDescending(group => group.LastReportedAt)
            .ToList();

        var totalCount = groupedReports.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);
        var pagedGroups = groupedReports
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        if (pagedGroups.Count == 0)
        {
            return new PagedResponseDto<AdminGroupedReportItemDto>(
                [],
                pagination.Page,
                pagination.PageSize,
                totalCount,
                totalPages
            );
        }

        /** Coleta os IDs dos alvos da página atual separados por tipo para busca em lote. */
        var placeIds = pagedGroups
            .Where(group => group.TargetType == ReportTargetType.Place)
            .Select(group => group.TargetId)
            .Distinct()
            .ToList();

        var reviewIds = pagedGroups
            .Where(group => group.TargetType == ReportTargetType.Review)
            .Select(group => group.TargetId)
            .Distinct()
            .ToList();

        var commentIds = pagedGroups
            .Where(group => group.TargetType == ReportTargetType.Comment)
            .Select(group => group.TargetId)
            .Distinct()
            .ToList();

        var placesById = placeIds.Count > 0
            ? await context.Places
                .AsNoTracking()
                .Include(place => place.Category)
                .Include(place => place.Author)
                .Where(place => placeIds.Contains(place.Id))
                .ToDictionaryAsync(place => place.Id, cancellationToken)
            : new Dictionary<Guid, Place>();

        var reviewsById = reviewIds.Count > 0
            ? await context.Reviews
                .AsNoTracking()
                .Include(review => review.Place)
                .Include(review => review.User)
                .Where(review => reviewIds.Contains(review.Id))
                .ToDictionaryAsync(review => review.Id, cancellationToken)
            : new Dictionary<Guid, Review>();

        var commentsById = commentIds.Count > 0
            ? await context.Comments
                .AsNoTracking()
                .Include(comment => comment.User)
                .Include(comment => comment.Review).ThenInclude(review => review.Place)
                .Where(comment => commentIds.Contains(comment.Id))
                .ToDictionaryAsync(comment => comment.Id, cancellationToken)
            : new Dictionary<Guid, Comment>();

        /** Coleta os IDs de todos os autores e denunciantes envolvidos na página para calcular reputação e histórico em lote. */
        var relevantUserIds = new HashSet<Guid>();

        foreach (var place in placesById.Values)
            if (place.AuthorId.HasValue)
                relevantUserIds.Add(place.AuthorId.Value);

        foreach (var review in reviewsById.Values)
            if (review.UserId.HasValue)
                relevantUserIds.Add(review.UserId.Value);

        foreach (var comment in commentsById.Values)
            if (comment.UserId.HasValue)
                relevantUserIds.Add(comment.UserId.Value);

        foreach (var group in pagedGroups)
            foreach (var report in group.Reports)
                relevantUserIds.Add(report.ReporterId);

        var publishedReviewsCountByUser = relevantUserIds.Count > 0
            ? await context.Reviews
                .AsNoTracking()
                .Where(review => review.UserId.HasValue && relevantUserIds.Contains(review.UserId.Value) && review.RemovedAt == null)
                .GroupBy(review => review.UserId!.Value)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        var removedReviewsCountByUser = relevantUserIds.Count > 0
            ? await context.Reviews
                .AsNoTracking()
                .Where(review => review.UserId.HasValue && relevantUserIds.Contains(review.UserId.Value) && review.RemovedAt != null)
                .GroupBy(review => review.UserId!.Value)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        var removedCommentsCountByUser = relevantUserIds.Count > 0
            ? await context.Comments
                .AsNoTracking()
                .Where(comment => comment.UserId.HasValue && relevantUserIds.Contains(comment.UserId.Value) && comment.RemovedAt != null)
                .GroupBy(comment => comment.UserId!.Value)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        var rejectedPlacesCountByUser = relevantUserIds.Count > 0
            ? await context.Places
                .AsNoTracking()
                .Where(place => place.AuthorId.HasValue && relevantUserIds.Contains(place.AuthorId.Value) && place.Status == PlaceStatus.Rejected)
                .GroupBy(place => place.AuthorId!.Value)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        var visitsCountByUser = relevantUserIds.Count > 0
            ? await context.Visits
                .AsNoTracking()
                .Where(visit => relevantUserIds.Contains(visit.UserId))
                .GroupBy(visit => visit.UserId)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        bool IsUserVerified(User? user)
        {
            if (user is null)
                return false;

            var visitsCount = visitsCountByUser.GetValueOrDefault(user.Id, 0);
            var publishedReviewsCount = publishedReviewsCountByUser.GetValueOrDefault(user.Id, 0);
            var removedReviewsCount = removedReviewsCountByUser.GetValueOrDefault(user.Id, 0);
            var accountAgeDays = Math.Max(0, (int)(now - user.CreatedAt).TotalDays);

            return visitsCount >= RequiredVisitsForVerification
                && publishedReviewsCount >= RequiredReviewsForVerification
                && removedReviewsCount <= MaxAllowedRemovedReviewsForVerification
                && accountAgeDays >= RequiredAccountAgeDaysForVerification;
        }

        int GetRemovedContentCount(Guid? userId)
        {
            if (!userId.HasValue)
                return 0;

            return removedReviewsCountByUser.GetValueOrDefault(userId.Value, 0)
                + removedCommentsCountByUser.GetValueOrDefault(userId.Value, 0)
                + rejectedPlacesCountByUser.GetValueOrDefault(userId.Value, 0);
        }

        var resultItems = new List<AdminGroupedReportItemDto>();

        foreach (var group in pagedGroups)
        {
            ReportTargetSnapshotDto? snapshot = null;

            switch (group.TargetType)
            {
                case ReportTargetType.Place:
                    if (placesById.TryGetValue(group.TargetId, out var place))
                    {
                        var author = place.Author;
                        var authorReviewsCount = author is not null ? publishedReviewsCountByUser.GetValueOrDefault(author.Id, 0) : 0;
                        var authorRemovedCount = GetRemovedContentCount(place.AuthorId);
                        var authorTitle = IsUserVerified(author) ? "Explorador Verificado" : "Explorador";

                        snapshot = new ReportTargetSnapshotDto(
                            place.Id,
                            ReportTargetType.Place,
                            place.Name,
                            place.Neighborhood,
                            place.About,
                            author is null ? null : new AuthorSummaryDto(author.Id, author.Name, author.AvatarUrl),
                            place.CreatedAt,
                            authorTitle,
                            author?.CreatedAt,
                            authorReviewsCount,
                            authorRemovedCount,
                            place.Name,
                            place.Neighborhood,
                            place.Category?.Name,
                            string.Format(CultureInfo.InvariantCulture, "{0:F4}, {1:F4}", place.Latitude, place.Longitude),
                            null,
                            null
                        );
                    }
                    break;

                case ReportTargetType.Review:
                    if (reviewsById.TryGetValue(group.TargetId, out var review))
                    {
                        var author = review.User;
                        var authorReviewsCount = author is not null ? publishedReviewsCountByUser.GetValueOrDefault(author.Id, 0) : 0;
                        var authorRemovedCount = GetRemovedContentCount(review.UserId);
                        var authorTitle = IsUserVerified(author) ? "Explorador Verificado" : "Explorador";

                        snapshot = new ReportTargetSnapshotDto(
                            review.Id,
                            ReportTargetType.Review,
                            $"Avaliação em {review.Place.Name}",
                            $"Nota: {review.Rating}/5",
                            review.Content,
                            author is null ? null : new AuthorSummaryDto(author.Id, author.Name, author.AvatarUrl),
                            review.CreatedAt,
                            authorTitle,
                            author?.CreatedAt,
                            authorReviewsCount,
                            authorRemovedCount,
                            review.Place?.Name,
                            review.Place?.Neighborhood,
                            null,
                            null,
                            review.Rating,
                            null
                        );
                    }
                    break;

                case ReportTargetType.Comment:
                    if (commentsById.TryGetValue(group.TargetId, out var comment))
                    {
                        var author = comment.User;
                        var authorReviewsCount = author is not null ? publishedReviewsCountByUser.GetValueOrDefault(author.Id, 0) : 0;
                        var authorRemovedCount = GetRemovedContentCount(comment.UserId);
                        var authorTitle = IsUserVerified(author) ? "Explorador Verificado" : "Explorador";

                        snapshot = new ReportTargetSnapshotDto(
                            comment.Id,
                            ReportTargetType.Comment,
                            $"Comentário em avaliação de {comment.Review.Place.Name}",
                            null,
                            comment.Content,
                            author is null ? null : new AuthorSummaryDto(author.Id, author.Name, author.AvatarUrl),
                            comment.CreatedAt,
                            authorTitle,
                            author?.CreatedAt,
                            authorReviewsCount,
                            authorRemovedCount,
                            comment.Review?.Place?.Name,
                            comment.Review?.Place?.Neighborhood,
                            null,
                            null,
                            comment.Review?.Rating,
                            comment.Review?.Content
                        );
                    }
                    break;
            }

            if (snapshot is null)
                continue;

            var reporters = group.Reports
                .Select(report =>
                {
                    var reporterUser = report.Reporter;
                    var reporterName = !string.IsNullOrWhiteSpace(reporterUser?.Name) ? reporterUser.Name.Trim() : "Explorador";
                    var reporterInitials = BuildInitials(reporterName);
                    var isReporterVerified = IsUserVerified(reporterUser);
                    var memberSince = isReporterVerified
                        ? "explorador verificado"
                        : reporterUser is not null
                            ? $"membro desde {reporterUser.CreatedAt.ToString("MMM yyyy", PtBrCulture)}"
                            : "membro recente";
                    var accountAgeDays = reporterUser is not null
                        ? Math.Max(0, (int)(now - reporterUser.CreatedAt).TotalDays)
                        : 30;

                    return new ReportReporterItemDto(
                        reporterName,
                        reporterInitials,
                        report.Reason,
                        report.CreatedAt,
                        memberSince,
                        accountAgeDays
                    );
                })
                .ToList();

            resultItems.Add(new AdminGroupedReportItemDto(
                group.TargetType,
                group.TargetId,
                group.ReportersCount,
                group.Reasons,
                snapshot,
                group.FirstReportedAt,
                group.LastReportedAt,
                reporters
            ));
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

    /**
     * Extrai até duas iniciais maiúsculas a partir do nome completo do usuário.
     */
    private static string BuildInitials(string fullName)
    {
        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
            return "EX";

        if (parts.Length == 1)
            return parts[0][..1].ToUpperInvariant();

        return string.Concat(parts[0][..1], parts[1][..1]).ToUpperInvariant();
    }
}
