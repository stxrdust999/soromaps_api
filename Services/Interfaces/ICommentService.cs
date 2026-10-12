using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface ICommentService
{
    Task<Result<PagedResponseDto<CommentResponseDto>>> ListByReviewIdAsync(Guid reviewId, PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<Result<CommentResponseDto>> CreateAsync(CreateCommentDto request, CancellationToken cancellationToken = default);
    Task<Result<CommentResponseDto>> UpdateAsync(Guid id, UpdateCommentDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
