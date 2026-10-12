using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Reviews;

public sealed record CreateReviewDto(
    [Required(ErrorMessage = "review.place_id_required")]
    Guid PlaceId,

    [Required(ErrorMessage = "review.content_required")]
    [MaxLength(2000, ErrorMessage = "review.content_too_long")]
    string Content,

    [Range(1, 5, ErrorMessage = "review.rating_out_of_range")]
    short Rating
);
