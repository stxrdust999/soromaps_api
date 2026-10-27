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
        var todayUtc = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
        var currentWindowStart = todayUtc.AddDays(-days);
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

        /** Materializa os timestamps em UTC e agrupa por dia em memória para evitar falha de tradução de `.Date` no Npgsql. */
        var userDates = await context.Users.AsNoTracking().Where(u => u.CreatedAt >= currentWindowStart).Select(u => u.CreatedAt).ToListAsync(cancellationToken);
        var reviewDates = await context.Reviews.AsNoTracking().Where(r => r.RemovedAt == null && r.CreatedAt >= currentWindowStart).Select(r => r.CreatedAt).ToListAsync(cancellationToken);
        var placeDates = await context.Places.AsNoTracking().Where(p => p.CreatedAt >= currentWindowStart).Select(p => p.CreatedAt).ToListAsync(cancellationToken);
        var decisionDates = await context.ModerationDecisions.AsNoTracking().Where(md => md.CreatedAt >= currentWindowStart).Select(md => md.CreatedAt).ToListAsync(cancellationToken);

        var userMap = userDates.GroupBy(timestamp => timestamp.Date).ToDictionary(group => group.Key, group => group.Count());
        var reviewMap = reviewDates.GroupBy(timestamp => timestamp.Date).ToDictionary(group => group.Key, group => group.Count());
        var placeMap = placeDates.GroupBy(timestamp => timestamp.Date).ToDictionary(group => group.Key, group => group.Count());
        var decisionMap = decisionDates.GroupBy(timestamp => timestamp.Date).ToDictionary(group => group.Key, group => group.Count());

        var dailySeries = new List<DailySeriesItemDto>();
        for (var i = days - 1; i >= 0; i--)
        {
            var dayDate = todayUtc.AddDays(-i).Date;
            var dateKey = dayDate.ToString("yyyy-MM-dd");

            dailySeries.Add(new DailySeriesItemDto(
                dateKey,
                userMap.GetValueOrDefault(dayDate, 0),
                reviewMap.GetValueOrDefault(dayDate, 0),
                placeMap.GetValueOrDefault(dayDate, 0),
                decisionMap.GetValueOrDefault(dayDate, 0)
            ));
        }

        /** Projeta contagens em tipo anônimo antes de mapear para record posicional, garantindo tradução SQL do OrderByDescending. */
        var rawCategoriesDistribution = await context.Categories
            .AsNoTracking()
            .Select(category => new
            {
                category.Name,
                category.Color,
                category.Icon,
                PlacesCount = category.Places.Count(place => place.Status == PlaceStatus.Approved),
                ReviewsCount = category.Places.SelectMany(place => place.Reviews).Count(review => review.RemovedAt == null)
            })
            .OrderByDescending(category => category.PlacesCount)
            .ToListAsync(cancellationToken);

        var categoriesDistribution = rawCategoriesDistribution
            .Select(category => new CategoryDistributionItemDto(
                category.Name,
                category.Color,
                category.Icon,
                category.PlacesCount,
                category.ReviewsCount
            ))
            .ToList();

        /** Busca os 5 locais pendentes mais antigos e calcula as horas em espera em memória. */
        var oldestPendingRaw = await context.Places
            .AsNoTracking()
            .Where(place => place.Status == PlaceStatus.Pending)
            .OrderBy(place => place.CreatedAt)
            .Take(5)
            .Select(place => new
            {
                place.Id,
                place.Name,
                place.Neighborhood,
                CategoryName = place.Category.Name,
                place.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var oldestPendingPlaces = oldestPendingRaw
            .Select(place => new PendingPlaceQueuePreviewDto(
                place.Id,
                place.Name,
                place.Neighborhood,
                place.CategoryName,
                place.CreatedAt,
                Math.Round(Math.Max(0, (now - place.CreatedAt).TotalHours), 1)
            ))
            .ToList();

        /** Materializa as denúncias abertas e consolida o agrupamento de motivos distintos em memória. */
        var openReportsRaw = await context.Reports
            .AsNoTracking()
            .Where(report => report.Status == ReportStatus.Open)
            .Select(report => new
            {
                report.TargetType,
                report.TargetId,
                report.ReporterId,
                report.Reason,
                report.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var topReportedGroups = openReportsRaw
            .GroupBy(report => new { report.TargetType, report.TargetId })
            .Select(group => new
            {
                group.Key.TargetType,
                group.Key.TargetId,
                ReportersCount = group.Select(report => report.ReporterId).Distinct().Count(),
                Reasons = group.Select(report => report.Reason).Distinct().ToList(),
                FirstReportedAt = group.Min(report => report.CreatedAt),
                LastReportedAt = group.Max(report => report.CreatedAt)
            })
            .OrderByDescending(group => group.ReportersCount)
            .ThenByDescending(group => group.LastReportedAt)
            .Take(5)
            .ToList();

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
