using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IReviewService
{
    Task<Result<PagedResponseDto<ReviewResponseDto>>> ListByPlaceIdAsync(Guid placeId, PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<Result<ReviewResponseDto>> CreateAsync(CreateReviewDto request, CancellationToken cancellationToken = default);
    Task<Result<ReviewResponseDto>> UpdateAsync(Guid id, UpdateReviewDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
