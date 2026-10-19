using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Common;

public sealed record CreateUploadUrlRequestDto(
    [MaxLength(15, ErrorMessage = "storage.extension_too_long")]
    string? FileExtension = "webp"
);

public sealed record SignedUploadUrlDto(
    string UploadUrl,
    string PublicUrl,
    string StoragePath,
    string Bucket
);
