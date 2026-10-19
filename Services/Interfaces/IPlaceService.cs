using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IPlaceService
{
    Task<Result<PagedResponseDto<PlaceSummaryDto>>> ListApprovedAsync(
        PaginationQueryDto pagination,
        PlaceListQueryDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<PlaceResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PlaceResponseDto>> CreateAsync(CreatePlaceDto request, CancellationToken cancellationToken = default);

    Task<Result<PlaceResponseDto>> UpdateAsync(Guid id, UpdatePlaceDto request, CancellationToken cancellationToken = default);

    Task<Result<PlaceResponseDto>> UpdateStatusAsync(Guid id, UpdatePlaceStatusDto request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<SignedUploadUrlDto>> CreatePhotoUploadUrlAsync(
        Guid placeId,
        CreateUploadUrlRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<PlacePhotoResponseDto>> AddPhotoAsync(
        Guid placeId,
        ConfirmPlacePhotoDto request,
        CancellationToken cancellationToken = default);

    Task<Result<PlacePhotoResponseDto>> SetCoverPhotoAsync(
        Guid placeId,
        Guid photoId,
        CancellationToken cancellationToken = default);

    Task<Result> DeletePhotoAsync(
        Guid placeId,
        Guid photoId,
        CancellationToken cancellationToken = default);
}
