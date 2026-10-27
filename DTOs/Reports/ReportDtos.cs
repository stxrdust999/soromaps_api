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

/**
 * Dados resumidos de um usuário denunciante em um caso agrupado de moderação.
 */
public sealed record ReportReporterItemDto(
    string Name,
    string Initials,
    string Reason,
    DateTime CreatedAt,
    string MemberSince,
    int AccountAgeDays
);

/**
 * Snapshot enriquecido do conteúdo denunciado e métricas históricas do autor.
 */
public sealed record ReportTargetSnapshotDto(
    Guid TargetId,
    ReportTargetType TargetType,
    string Title,
    string? Subtitle,
    string Content,
    AuthorSummaryDto? Author,
    DateTime CreatedAt,
    string? AuthorTitle = null,
    DateTime? AuthorMemberSince = null,
    int AuthorReviewsCount = 0,
    int AuthorRemovedContentCount = 0,
    string? PlaceName = null,
    string? Neighborhood = null,
    string? CategoryName = null,
    string? Coordinates = null,
    int? Rating = null,
    string? ParentReviewContent = null
);

/**
 * Caso agrupado de denúncias abertas sobre um mesmo alvo polimórfico.
 */
public sealed record AdminGroupedReportItemDto(
    ReportTargetType TargetType,
    Guid TargetId,
    int ReportersCount,
    IReadOnlyList<string> Reasons,
    ReportTargetSnapshotDto Target,
    DateTime FirstReportedAt,
    DateTime LastReportedAt,
    IReadOnlyList<ReportReporterItemDto>? Reporters = null
);

public sealed record DismissReportsDto(
    [MaxLength(300, ErrorMessage = "report.resolution_note_too_long")]
    string? ResolutionNote
);
