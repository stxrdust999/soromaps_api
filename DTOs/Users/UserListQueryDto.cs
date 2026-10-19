using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Users;

public sealed record UserListQueryDto(
    UserRole? Role = null,
    string? Search = null
);
