using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Moderation;

public sealed record ModerationQueueQueryDto(
    PlaceStatus? Status = PlaceStatus.Pending,
    Guid? CategoryId = null,
    string? Search = null
);

public sealed record ModerationAuthorStatsDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int TotalSubmittedPlaces,
    int ApprovedPlacesCount,
    DateTime JoinedAt
);

public sealed record DuplicatePlaceHintDto(
    Guid Id,
    string Name,
    string Neighborhood,
    double DistanceMeters,
    PlaceStatus Status
);

public sealed record ModerationPlaceItemDto(
    Guid Id,
    CategorySummaryDto Category,
    ModerationAuthorStatsDto? Author,
    string Name,
    double Latitude,
    double Longitude,
    string Neighborhood,
    string About,
    string Description,
    bool HasWifi,
    bool IsPetFriendly,
    string? BestTime,
    string? LocalSecret,
    IReadOnlyList<PlacePhotoResponseDto> Photos,
    int CompletenessPercentage,
    IReadOnlyList<DuplicatePlaceHintDto> PossibleDuplicates,
    PlaceStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record ModerationDecisionRequestDto(
    [Required(ErrorMessage = "moderation.decision_required")]
    PlaceStatus Decision,

    PlaceStatus? ExpectedCurrentStatus = null,

    [MaxLength(50, ErrorMessage = "moderation.reason_too_long")]
    string? Reason = null,

    [MaxLength(500, ErrorMessage = "moderation.note_too_long")]
    string? FeedbackNote = null
);

public sealed record BatchModerationDecisionRequestDto(
    [Required(ErrorMessage = "moderation.place_ids_required")]
    [MinLength(1, ErrorMessage = "moderation.place_ids_empty")]
    IReadOnlyList<Guid> PlaceIds,

    [Required(ErrorMessage = "moderation.decision_required")]
    PlaceStatus Decision,

    [MaxLength(50, ErrorMessage = "moderation.reason_too_long")]
    string? Reason = null,

    [MaxLength(500, ErrorMessage = "moderation.note_too_long")]
    string? FeedbackNote = null
);

public sealed record ModerationDecisionResponseDto(
    Guid Id,
    Guid PlaceId,
    string PlaceName,
    AuthorSummaryDto? Moderator,
    PlaceStatus PreviousStatus,
    PlaceStatus Decision,
    string? Reason,
    string? FeedbackNote,
    bool IsUndo,
    DateTime CreatedAt
);
