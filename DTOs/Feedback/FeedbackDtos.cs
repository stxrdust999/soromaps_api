using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Common;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Feedback;

public sealed record CreateFeedbackDto(
    [Required(ErrorMessage = "feedback.category_required")]
    FeedbackCategory Category,

    [MaxLength(120, ErrorMessage = "feedback.origin_path_too_long")]
    string? OriginPath,

    [Required(ErrorMessage = "feedback.message_required")]
    [MaxLength(1000, ErrorMessage = "feedback.message_too_long")]
    string Message
);

public sealed record FeedbackResponseDto(
    Guid Id,
    AuthorSummaryDto Author,
    FeedbackCategory Category,
    string? OriginPath,
    string Message,
    DateTime CreatedAt
);
