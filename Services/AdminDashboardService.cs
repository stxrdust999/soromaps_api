using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Admin;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reports;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por consolidar métricas, indicadores e séries temporais do dashboard administrativo.
 */
public sealed class AdminDashboardService(
    AppDbContext context,
    TimeProvider timeProvider) : IAdminDashboardService
{
    private const double OutlierVarianceThreshold = 2.0;
    private const int OutlierMinimumReviewsThreshold = 5;
    private const int BurstReviewsThreshold = 3;
    private static readonly TimeSpan BurstWindow = TimeSpan.FromHours(24);

    /**
     * Consolida filas de atenção operacional, KPIs de crescimento com comparativo anterior e séries diárias completas.
     */
    public async Task<Result<AdminDashboardStatsDto>> GetStatsAsync(string period, CancellationToken cancellationToken = default)
    {
        var days = period.ToLowerInvariant() switch
        {
            "7d" => 7,
            "90d" => 90,
            _ => 30
        };

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var currentWindowStart = now.AddDays(-days);
        var previousWindowStart = currentWindowStart.AddDays(-days);

        // 1. Filas de atenção
        var pendingPlacesCount = await context.Places.CountAsync(p => p.Status == PlaceStatus.Pending, cancellationToken);
        var fortyEightHoursAgo = now.AddHours(-48);
        var pendingPlacesOver48hCount = await context.Places.CountAsync(p => p.Status == PlaceStatus.Pending && p.CreatedAt <= fortyEightHoursAgo, cancellationToken);

        var openReportsTargetsCount = await context.Reports
            .Where(r => r.Status == ReportStatus.Open)
            .Select(r => new { r.TargetType, r.TargetId })
            .Distinct()
            .CountAsync(cancellationToken);

        var flaggedReviewsCount = await CalculateFlaggedReviewsCountAsync(now, cancellationToken);

        var queues = new AttentionQueuesDto(
            pendingPlacesCount,
            pendingPlacesOver48hCount,
            openReportsTargetsCount,
            flaggedReviewsCount
        );

        // 2. KPIs
        var currentActivePlaces = await context.Places.CountAsync(p => p.Status == PlaceStatus.Approved && p.CreatedAt >= currentWindowStart, cancellationToken);
        var previousActivePlaces = await context.Places.CountAsync(p => p.Status == PlaceStatus.Approved && p.CreatedAt >= previousWindowStart && p.CreatedAt < currentWindowStart, cancellationToken);

        var currentNewUsers = await context.Users.CountAsync(u => u.CreatedAt >= currentWindowStart, cancellationToken);
        var previousNewUsers = await context.Users.CountAsync(u => u.CreatedAt >= previousWindowStart && u.CreatedAt < currentWindowStart, cancellationToken);

        var currentNewReviews = await context.Reviews.CountAsync(r => r.RemovedAt == null && r.CreatedAt >= currentWindowStart, cancellationToken);
        var previousNewReviews = await context.Reviews.CountAsync(r => r.RemovedAt == null && r.CreatedAt >= previousWindowStart && r.CreatedAt < currentWindowStart, cancellationToken);

        var moderationTimeRecords = await context.ModerationDecisions
            .AsNoTracking()
            .Where(md => md.CreatedAt >= currentWindowStart)
            .Select(md => new { md.CreatedAt, PlaceCreatedAt = md.Place.CreatedAt })
            .ToListAsync(cancellationToken);

        var avgModHours = moderationTimeRecords.Count > 0
            ? Math.Round(moderationTimeRecords.Average(md => Math.Max(0, (md.CreatedAt - md.PlaceCreatedAt).TotalHours)), 1)
            : 0.0;

        var kpis = new KpisDto(
            new KpiMetricDto(currentActivePlaces, previousActivePlaces, CalculatePctChange(currentActivePlaces, previousActivePlaces)),
            new KpiMetricDto(currentNewUsers, previousNewUsers, CalculatePctChange(currentNewUsers, previousNewUsers)),
            new KpiMetricDto(currentNewReviews, previousNewReviews, CalculatePctChange(currentNewReviews, previousNewReviews)),
            avgModHours
        );

        // 3. Séries temporais diárias
        var userDates = await context.Users.AsNoTracking().Where(u => u.CreatedAt >= currentWindowStart).Select(u => u.CreatedAt.Date).ToListAsync(cancellationToken);
        var reviewDates = await context.Reviews.AsNoTracking().Where(r => r.RemovedAt == null && r.CreatedAt >= currentWindowStart).Select(r => r.CreatedAt.Date).ToListAsync(cancellationToken);
        var placeDates = await context.Places.AsNoTracking().Where(p => p.CreatedAt >= currentWindowStart).Select(p => p.CreatedAt.Date).ToListAsync(cancellationToken);
        var decisionDates = await context.ModerationDecisions.AsNoTracking().Where(md => md.CreatedAt >= currentWindowStart).Select(md => md.CreatedAt.Date).ToListAsync(cancellationToken);

        var userMap = userDates.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var reviewMap = reviewDates.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var placeMap = placeDates.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        var decisionMap = decisionDates.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());

        var dailySeries = new List<DailySeriesItemDto>();
        for (var i = days - 1; i >= 0; i--)
        {
            var dayDate = now.Date.AddDays(-i);
            var dateKey = dayDate.ToString("yyyy-MM-dd");

            dailySeries.Add(new DailySeriesItemDto(
                dateKey,
                userMap.GetValueOrDefault(dayDate, 0),
                reviewMap.GetValueOrDefault(dayDate, 0),
                placeMap.GetValueOrDefault(dayDate, 0),
                decisionMap.GetValueOrDefault(dayDate, 0)
            ));
        }

        // 4. Distribuição por categoria
        var categoriesDistribution = await context.Categories
            .AsNoTracking()
            .Select(c => new CategoryDistributionItemDto(
                c.Name,
                c.Color,
                c.Icon,
                c.Places.Count(p => p.Status == PlaceStatus.Approved),
                c.Places.SelectMany(p => p.Reviews).Count(r => r.RemovedAt == null)
            ))
            .OrderByDescending(c => c.PlacesCount)
            .ToListAsync(cancellationToken);

        // 5. Itens recentes da fila
        var oldestPendingPlaces = await context.Places
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Status == PlaceStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PendingPlaceQueuePreviewDto(
                p.Id,
                p.Name,
                p.Neighborhood,
                p.Category.Name,
                p.CreatedAt,
                Math.Round((now - p.CreatedAt).TotalHours, 1)
            ))
            .ToListAsync(cancellationToken);

        // 6. Top alvos denunciados
        var topReportedGroups = await context.Reports
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
            .Take(5)
            .ToListAsync(cancellationToken);

        var topReportedTargets = new List<AdminGroupedReportItemDto>();
        foreach (var group in topReportedGroups)
        {
            var snapshot = await BuildTargetSnapshotAsync(group.TargetType, group.TargetId, cancellationToken);
            if (snapshot is not null)
            {
                topReportedTargets.Add(new AdminGroupedReportItemDto(
                    group.TargetType,
                    group.TargetId,
                    group.ReportersCount,
                    group.Reasons,
                    snapshot,
                    group.FirstReportedAt,
                    group.LastReportedAt
                ));
            }
        }

        return new AdminDashboardStatsDto(
            $"{days}d",
            queues,
            kpis,
            dailySeries,
            categoriesDistribution,
            oldestPendingPlaces,
            topReportedTargets
        );
    }

    private async Task<int> CalculateFlaggedReviewsCountAsync(DateTime now, CancellationToken cancellationToken)
    {
        var placeAverages = await context.Reviews
            .AsNoTracking()
            .Where(r => r.RemovedAt == null)
            .GroupBy(r => r.PlaceId)
            .Select(g => new
            {
                PlaceId = g.Key,
                Average = g.Average(r => (double)r.Rating),
                Total = g.Count()
            })
            .ToDictionaryAsync(item => item.PlaceId, cancellationToken);

        var recentAuthorBursts = await context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId.HasValue && r.CreatedAt >= now - BurstWindow)
            .GroupBy(r => r.UserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);

        var openReportReviewIds = await context.Reports
            .AsNoTracking()
            .Where(r => r.TargetType == ReportTargetType.Review && r.Status == ReportStatus.Open)
            .Select(r => r.TargetId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reportedReviewSet = openReportReviewIds.ToHashSet();

        var candidateReviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.RemovedAt == null)
            .Select(r => new { r.Id, r.PlaceId, r.UserId, r.Rating })
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var review in candidateReviews)
        {
            var isOutlier = placeAverages.TryGetValue(review.PlaceId, out var stats)
                && stats.Total >= OutlierMinimumReviewsThreshold
                && Math.Abs(review.Rating - stats.Average) >= OutlierVarianceThreshold;

            var isBurst = review.UserId.HasValue
                && recentAuthorBursts.TryGetValue(review.UserId.Value, out var burstTotal)
                && burstTotal >= BurstReviewsThreshold;

            var isReported = reportedReviewSet.Contains(review.Id);

            if (isOutlier || isBurst || isReported)
                count++;
        }

        return count;
    }

    private static double CalculatePctChange(int current, int previous)
    {
        if (previous == 0)
            return current > 0 ? 100.0 : 0.0;

        return Math.Round(((current - previous) / (double)previous) * 100.0, 1);
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
