using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class CommentService(
    AppDbContext context,
    ICurrentUserService currentUser,
    IAchievementEvaluatorService evaluatorService) : ICommentService
{
    public async Task<Result<PagedResponseDto<CommentResponseDto>>> ListByReviewIdAsync(
        Guid reviewId,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        return await context.Comments
            .AsNoTracking()
            .Where(comment => comment.ReviewId == reviewId && comment.RemovedAt == null)
            .OrderBy(comment => comment.CreatedAt)
            .Select(CommentMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<CommentResponseDto>> CreateAsync(CreateCommentDto request, CancellationToken cancellationToken = default)
    {
        var reviewExists = await context.Reviews
            .AnyAsync(r => r.Id == request.ReviewId && r.RemovedAt == null, cancellationToken);

        if (!reviewExists)
            return CommentErrors.ReviewNotFound;

        if (request.ReplyToId.HasValue)
        {
            var parentComment = await context.Comments
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == request.ReplyToId.Value && c.RemovedAt == null, cancellationToken);

            if (parentComment is null)
                return CommentErrors.ParentCommentNotFound;

            if (parentComment.ReplyToId.HasValue)
                return CommentErrors.MaxDepthExceeded;
        }

        var comment = new Comment
        {
            ReviewId = request.ReviewId,
            UserId = currentUser.UserId,
            ReplyToId = request.ReplyToId,
            Content = request.Content.Trim()
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        await evaluatorService.EvaluateForUserAsync(currentUser.UserId, AchievementEventType.Comment, cancellationToken);

        await context.Entry(comment).Reference(c => c.User).LoadAsync(cancellationToken);

        return comment.ToResponse();
    }

    public async Task<Result<CommentResponseDto>> UpdateAsync(Guid id, UpdateCommentDto request, CancellationToken cancellationToken = default)
    {
        var comment = await context.Comments
            .Include(existingComment => existingComment.User)
            .SingleOrDefaultAsync(existingComment => existingComment.Id == id && existingComment.RemovedAt == null, cancellationToken);

        if (comment is null)
            return CommentErrors.NotFound;

        var isOwner = comment.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return CommentErrors.NotOwner;

        comment.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return comment.ToResponse();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await context.Comments
            .SingleOrDefaultAsync(existingComment => existingComment.Id == id && existingComment.RemovedAt == null, cancellationToken);

        if (comment is null)
            return CommentErrors.NotFound;

        var isOwner = comment.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return CommentErrors.NotOwner;

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
