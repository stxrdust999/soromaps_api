using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Common;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Reports;

public sealed record CreateReportDto(
    [Required(ErrorMessage = "report.target_type_required")]
    ReportTargetType TargetType,

    [Required(ErrorMessage = "report.target_id_required")]
    Guid TargetId,

    [Required(ErrorMessage = "report.reason_required")]
    [MaxLength(40, ErrorMessage = "report.reason_too_long")]
    string Reason,

    [MaxLength(500, ErrorMessage = "report.details_too_long")]
    string? Details
);

public sealed record ReportTargetSnapshotDto(
    Guid TargetId,
    ReportTargetType TargetType,
    string Title,
    string? Subtitle,
    string Content,
    AuthorSummaryDto? Author,
    DateTime CreatedAt
);

public sealed record AdminGroupedReportItemDto(
    ReportTargetType TargetType,
    Guid TargetId,
    int ReportersCount,
    IReadOnlyList<string> Reasons,
    ReportTargetSnapshotDto Target,
    DateTime FirstReportedAt,
    DateTime LastReportedAt
);

public sealed record DismissReportsDto(
    [MaxLength(300, ErrorMessage = "report.resolution_note_too_long")]
    string? ResolutionNote
);
