using soromaps_api.DTOs.Auth;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, string? userAgent, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, string? userAgent, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAllAsync(CancellationToken cancellationToken = default);
}
