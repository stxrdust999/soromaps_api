using System.Security.Claims;
using soromaps_api.Models.Enums;

namespace soromaps_api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(idClaim, out var userId))
            return userId;

        return Guid.Empty;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("admin") || principal.IsInRole(nameof(UserRole.Admin).ToLowerInvariant());
    }
}
