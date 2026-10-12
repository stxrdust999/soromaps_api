using Microsoft.AspNetCore.Http;
using soromaps_api.Extensions;
using soromaps_api.Models.Enums;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly HttpContext? httpContext = httpContextAccessor.HttpContext;

    public Guid UserId => httpContext?.User.GetUserId() ?? Guid.Empty;

    public bool IsAuthenticated => httpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(UserRole role)
    {
        if (httpContext?.User == null)
            return false;

        var roleName = role.ToString().ToLowerInvariant();

        return httpContext.User.IsInRole(roleName);
    }
}
