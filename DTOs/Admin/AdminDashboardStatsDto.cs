using soromaps_api.DTOs.Reports;

namespace soromaps_api.DTOs.Admin;

public sealed record AttentionQueuesDto(
    int PendingPlacesCount,
    int PendingPlacesOver48hCount,
    int OpenReportsTargetsCount,
    int FlaggedReviewsCount
);

public sealed record KpiMetricDto(
    int Current,
    int Previous,
    double PercentageChange
);

public sealed record KpisDto(
    KpiMetricDto ActivePlaces,
    KpiMetricDto NewUsers,
    KpiMetricDto NewReviews,
    double AverageModerationTimeHours
);

public sealed record DailySeriesItemDto(
    string Date, // "yyyy-MM-dd"
    int NewUsers,
    int NewReviews,
    int NewPlaces,
    int ModerationDecisions
);

public sealed record CategoryDistributionItemDto(
    string CategoryName,
    string Color,
    string Icon,
    int PlacesCount,
    int ReviewsCount
);

public sealed record PendingPlaceQueuePreviewDto(
    Guid Id,
    string Name,
    string Neighborhood,
    string CategoryName,
    DateTime CreatedAt,
    double HoursPending
);

public sealed record AdminDashboardStatsDto(
    string Period, // "7d" | "30d" | "90d"
    AttentionQueuesDto Queues,
    KpisDto Kpis,
    IReadOnlyList<DailySeriesItemDto> DailySeries,
    IReadOnlyList<CategoryDistributionItemDto> CategoriesDistribution,
    IReadOnlyList<PendingPlaceQueuePreviewDto> OldestPendingPlaces,
    IReadOnlyList<AdminGroupedReportItemDto> TopReportedTargets
);
