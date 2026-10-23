using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class ReviewService(
    AppDbContext context,
    ICurrentUserService currentUser,
    IAchievementEvaluatorService evaluatorService) : IReviewService
{
    public async Task<Result<PagedResponseDto<ReviewResponseDto>>> ListByPlaceIdAsync(
        Guid placeId,
        PaginationQueryDto pagination,
        string? sort = "useful",
        CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.IsAuthenticated ? currentUser.UserId : Guid.Empty;

        var query = context.Reviews
            .AsNoTracking()
            .Where(review => review.PlaceId == placeId && review.RemovedAt == null);

        var sortedQuery = (sort?.ToLowerInvariant()) switch
        {
            "recent" => query.OrderByDescending(r => r.CreatedAt),
            "rating_desc" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            "rating_asc" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.UsefulVotes.Count).ThenByDescending(r => r.CreatedAt)
        };

        return await sortedQuery
            .Select(ReviewMappings.ToResponseProjection(currentUserId))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<PagedResponseDto<ReviewResponseDto>>> ListMyReviewsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        return await context.Reviews
            .AsNoTracking()
            .Where(review => review.UserId == currentUserId && review.RemovedAt == null)
            .OrderByDescending(review => review.CreatedAt)
            .Select(ReviewMappings.ToResponseProjection(currentUserId))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<ReviewResponseDto>> CreateAsync(CreateReviewDto request, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == request.PlaceId, cancellationToken);

        if (place is null || place.Status != PlaceStatus.Approved)
            return ReviewErrors.PlaceNotFound;

        var currentUserId = currentUser.UserId;

        var alreadyReviewed = await context.Reviews
            .AnyAsync(r => r.PlaceId == request.PlaceId && r.UserId == currentUserId && r.RemovedAt == null, cancellationToken);

        if (alreadyReviewed)
            return ReviewErrors.AlreadyExists;

        var review = new Review
        {
            PlaceId = request.PlaceId,
            UserId = currentUserId,
            Content = request.Content.Trim(),
            Rating = request.Rating
        };

        try
        {
            context.Reviews.Add(review);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return ReviewErrors.AlreadyExists;
        }

        await evaluatorService.EvaluateForUserAsync(currentUserId, AchievementEventType.Review, cancellationToken);

        var authorData = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == currentUserId)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.AvatarUrl,
                AchievementsCount = u.UserAchievements.Count,
                IsVerified = u.Visits.Count >= 5 &&
                             u.Reviews.Count(r => r.RemovedAt == null) >= 3 &&
                             !u.Reviews.Any(r => r.RemovedAt != null) &&
                             u.CreatedAt <= DateTime.UtcNow.AddDays(-30),
                VisitedPlace = u.Visits.Any(v => v.PlaceId == request.PlaceId)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new ReviewResponseDto(
            review.Id,
            review.PlaceId,
            authorData == null
                ? null
                : new ReviewAuthorDto(
                    authorData.Id,
                    authorData.Name,
                    authorData.AvatarUrl,
                    authorData.AchievementsCount,
                    authorData.IsVerified,
                    authorData.VisitedPlace
                ),
            review.Content,
            review.Rating,
            0,
            false,
            0,
            review.CreatedAt,
            review.UpdatedAt
        );
    }

    public async Task<Result<ReviewResponseDto>> UpdateAsync(Guid id, UpdateReviewDto request, CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews
            .Include(existingReview => existingReview.User)
            .SingleOrDefaultAsync(existingReview => existingReview.Id == id && existingReview.RemovedAt == null, cancellationToken);

        if (review is null)
            return ReviewErrors.NotFound;

        var isOwner = review.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return ReviewErrors.NotOwner;

        review.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews
            .SingleOrDefaultAsync(existingReview => existingReview.Id == id && existingReview.RemovedAt == null, cancellationToken);

        if (review is null)
            return ReviewErrors.NotFound;

        var isOwner = review.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return ReviewErrors.NotOwner;

        context.Reviews.Remove(review);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> VoteUsefulAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == id && r.RemovedAt == null, cancellationToken);

        if (review is null)
            return ReviewErrors.NotFound;

        if (review.UserId == currentUser.UserId)
            return ReviewErrors.CannotVoteOwn;

        var voteExists = await context.ReviewUsefulVotes
            .AnyAsync(v => v.ReviewId == id && v.UserId == currentUser.UserId, cancellationToken);

        if (!voteExists)
        {
            context.ReviewUsefulVotes.Add(new ReviewUsefulVote
            {
                ReviewId = id,
                UserId = currentUser.UserId
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> RemoveUsefulVoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vote = await context.ReviewUsefulVotes
            .SingleOrDefaultAsync(v => v.ReviewId == id && v.UserId == currentUser.UserId, cancellationToken);

        if (vote is null)
            return ReviewErrors.NotVoted;

        context.ReviewUsefulVotes.Remove(vote);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<ReviewResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = currentUser.IsAuthenticated ? currentUser.UserId : Guid.Empty;

        var response = await context.Reviews
            .AsNoTracking()
            .Where(review => review.Id == id && review.RemovedAt == null)
            .Select(ReviewMappings.ToResponseProjection(currentUserId))
            .SingleOrDefaultAsync(cancellationToken);

        if (response is null)
            return ReviewErrors.NotFound;

        return response;
    }
}
