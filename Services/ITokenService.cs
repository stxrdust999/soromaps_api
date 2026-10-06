using soromaps_api.Models;

namespace soromaps_api.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string HashToken(String token);
    }
}
