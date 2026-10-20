using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Common;

namespace soromaps_api.DTOs.Admin;

public sealed record AdminReviewQueryDto(
    string? Signal = null, // "outlier" | "burst" | "all"
    int? Rating = null,
    Guid? PlaceId = null,
    string? Search = null
);

public sealed record AdminReviewItemDto(
    Guid Id,
    Guid PlaceId,
    string PlaceName,
    AuthorSummaryDto? Author,
    string Content,
    int Rating,
    double PlaceRatingAverage,
    int UsefulVotesCount,
    IReadOnlyList<string> Signals,
    DateTime CreatedAt
);

public sealed record ContentRemovalRequestDto(
    [Required(ErrorMessage = "removal.target_type_required")]
    string TargetType, // "review" | "comment"

    [Required(ErrorMessage = "removal.target_id_required")]
    Guid TargetId,

    [Required(ErrorMessage = "removal.reason_required")]
    [MaxLength(50, ErrorMessage = "removal.reason_too_long")]
    string Reason,

    [MaxLength(500, ErrorMessage = "removal.note_too_long")]
    string? Note
);

public sealed record ContentRemovalResponseDto(
    Guid Id,
    string TargetType,
    Guid TargetId,
    AuthorSummaryDto? Author,
    AuthorSummaryDto? Moderator,
    string Reason,
    string? Note,
    string SnapshotText,
    DateTime CreatedAt
);
