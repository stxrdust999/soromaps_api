using System.Linq.Expressions;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class ReviewMappings
{
    public static Expression<Func<Review, ReviewResponseDto>> ToResponseProjection(Guid currentUserId) =>
        review => new ReviewResponseDto(
            review.Id,
            review.PlaceId,
            review.User == null
                ? null
                : new ReviewAuthorDto(
                    review.User.Id,
                    review.User.Name,
                    review.User.AvatarUrl,
                    review.User.UserAchievements.Count,
                    review.User.Visits.Count >= 5 &&
                    review.User.Reviews.Count(r => r.RemovedAt == null) >= 3 &&
                    !review.User.Reviews.Any(r => r.RemovedAt != null) &&
                    review.User.CreatedAt <= DateTime.UtcNow.AddDays(-30),
                    review.User.Visits.Any(v => v.PlaceId == review.PlaceId)
                ),
            review.Content,
            review.Rating,
            review.UsefulVotes.Count,
            review.UsefulVotes.Any(v => v.UserId == currentUserId),
            review.Comments.Count(c => c.RemovedAt == null),
            review.CreatedAt,
            review.UpdatedAt,
            review.Place.Name,
            review.Place.Category.Name
        );

    public static ReviewResponseDto ToResponse(
        this Review review,
        Guid currentUserId,
        int authorAchievementsCount = 0,
        bool isVerified = false,
        bool visitedPlace = false) =>
        new(
            review.Id,
            review.PlaceId,
            review.User == null
                ? null
                : new ReviewAuthorDto(
                    review.User.Id,
                    review.User.Name,
                    review.User.AvatarUrl,
                    authorAchievementsCount,
                    isVerified,
                    visitedPlace
                ),
            review.Content,
            review.Rating,
            review.UsefulVotes?.Count ?? 0,
            review.UsefulVotes?.Any(v => v.UserId == currentUserId) ?? false,
            review.Comments?.Count(c => c.RemovedAt == null) ?? 0,
            review.CreatedAt,
            review.UpdatedAt
        );

    public static void ApplyUpdate(this Review review, UpdateReviewDto request)
    {
        if (request.Content is not null)
            review.Content = request.Content.Trim();

        if (request.Rating.HasValue)
            review.Rating = request.Rating.Value;
    }
}
