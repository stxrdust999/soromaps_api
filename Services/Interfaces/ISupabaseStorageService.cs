using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface ISupabaseStorageService
{
    Task<Result<SignedUploadUrlDto>> CreatePlacePhotoUploadUrlAsync(
        Guid placeId,
        string? fileExtension = "webp",
        CancellationToken cancellationToken = default);

    Task<Result<SignedUploadUrlDto>> CreateAvatarUploadUrlAsync(
        Guid userId,
        string? fileExtension = "webp",
        CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(
        string bucket,
        string storagePath,
        CancellationToken cancellationToken = default);
}
