using soromaps_api.Models.Enums;

namespace soromaps_api.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(UserRole role);
}
