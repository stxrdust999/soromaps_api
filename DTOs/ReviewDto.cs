using soromaps_api.Models;

namespace soromaps_api.DTOs
{
    public record ReviewResponseDto(
          Guid ReviewId,
          Guid PlaceId,
          Guid UserId,
          string ReviewContent,
          float QtdStars
      )
    {
        public static ReviewResponseDto FromEntity(Review review) =>
            new(
                review.ReviewId,
                review.PlaceId,
                review.UserId,
                review.ReviewContent,
                review.QtdStars
            );
    }

    public record CreateReviewDto(
        Guid PlaceId,
        string ReviewContent,
        float QtdStars
    );

    public record UpdateReviewDto(
        string? ReviewContent,
        float? QtdStars
    );
}
