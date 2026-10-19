using System.Linq.Expressions;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class UserMappings
{
    public static readonly Expression<Func<User, UserResponseDto>> ToResponseProjection = user =>
        new UserResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString().ToLowerInvariant(),
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    public static readonly Expression<Func<User, PublicUserProfileDto>> ToPublicProfileProjection = user =>
        new PublicUserProfileDto(
            user.Id,
            user.Name,
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    public static readonly Expression<Func<User, AuthorSummaryDto>> ToAuthorSummaryProjection = user =>
        new AuthorSummaryDto(
            user.Id,
            user.Name,
            user.AvatarUrl
        );

    public static UserResponseDto ToResponse(this User user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString().ToLowerInvariant(),
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    public static PublicUserProfileDto ToPublicProfile(this User user) =>
        new(
            user.Id,
            user.Name,
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    public static AuthorSummaryDto ToAuthorSummary(this User user) =>
        new(
            user.Id,
            user.Name,
            user.AvatarUrl
        );

    public static void ApplyUpdate(this User user, UpdateUserDto request)
    {
        if (request.Name is not null)
            user.Name = request.Name.Trim();

        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl.Trim();

        if (request.Biography is not null)
            user.Biography = request.Biography.Trim();

        if (request.Neighborhood is not null)
            user.Neighborhood = request.Neighborhood.Trim();
    }

    public static void ApplyAdminUpdate(this User user, AdminUpdateUserDto request)
    {
        if (request.Name is not null)
            user.Name = request.Name.Trim();

        if (request.Email is not null)
            user.Email = request.Email.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        if (request.Role.HasValue)
            user.Role = request.Role.Value;

        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl.Trim();

        if (request.Biography is not null)
            user.Biography = request.Biography.Trim();

        if (request.Neighborhood is not null)
            user.Neighborhood = request.Neighborhood.Trim();
    }
}
