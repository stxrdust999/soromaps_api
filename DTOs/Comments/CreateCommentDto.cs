using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Comments;

public sealed record CreateCommentDto(
    [Required(ErrorMessage = "comment.review_id_required")]
    Guid ReviewId,

    [Required(ErrorMessage = "comment.content_required")]
    [MaxLength(1000, ErrorMessage = "comment.content_too_long")]
    string Content
);
