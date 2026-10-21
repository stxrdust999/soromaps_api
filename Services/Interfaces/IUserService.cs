using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserResponseDto>> RegisterAsync(RegisterUserDto request, CancellationToken cancellationToken = default);
    Task<Result<UserResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<PublicUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PagedResponseDto<UserResponseDto>>> ListAsync(PaginationQueryDto pagination, UserListQueryDto query, CancellationToken cancellationToken = default);
    Task<Result<UserResponseDto>> UpdateCurrentAsync(UpdateUserDto request, CancellationToken cancellationToken = default);
    Task<Result<SignedUploadUrlDto>> CreateAvatarUploadUrlAsync(CreateUploadUrlRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteCurrentAsync(CancellationToken cancellationToken = default);
    Task<Result<UserResponseDto>> AdminGetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserResponseDto>> AdminUpdateAsync(Guid userId, AdminUpdateUserDto request, CancellationToken cancellationToken = default);
    Task<Result> AdminDeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
