using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Reviews;

public sealed record UpdateReviewDto(
    [MaxLength(2000, ErrorMessage = "review.content_too_long")]
    string? Content,

    [Range(1, 5, ErrorMessage = "review.rating_out_of_range")]
    short? Rating
);
