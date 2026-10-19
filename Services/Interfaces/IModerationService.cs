using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Moderation;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IModerationService
{
    Task<Result<PagedResponseDto<ModerationPlaceItemDto>>> ListQueueAsync(
        PaginationQueryDto pagination,
        ModerationQueueQueryDto query,
        CancellationToken cancellationToken = default);

    Task<Result<ModerationDecisionResponseDto>> DecideAsync(
        Guid placeId,
        ModerationDecisionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ModerationDecisionResponseDto>>> BatchDecideAsync(
        BatchModerationDecisionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<ModerationDecisionResponseDto>> UndoAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResponseDto<ModerationDecisionResponseDto>>> ListHistoryAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default);
}
