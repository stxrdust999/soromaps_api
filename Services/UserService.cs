using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class UserService(
    AppDbContext context,
    ICurrentUserService currentUser,
    ISupabaseStorageService storageService,
    TimeProvider timeProvider) : IUserService
{
    public async Task<Result<UserResponseDto>> RegisterAsync(RegisterUserDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await context.Users
            .AnyAsync(existingUser => existingUser.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            return UserErrors.EmailTaken;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.Explorer
        };

        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return UserErrors.EmailTaken;
        }

        return user.ToResponse();
    }

    public async Task<Result<UserResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        return user.ToResponse();
    }

    public async Task<Result<PublicUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await context.Users
            .AsNoTracking()
            .Where(existingUser => existingUser.Id == userId)
            .Select(UserMappings.ToPublicProfileProjection)
            .SingleOrDefaultAsync(cancellationToken);

        if (profile is null)
            return UserErrors.NotFound;

        return profile;
    }

    public async Task<Result<PagedResponseDto<UserResponseDto>>> ListAsync(
        PaginationQueryDto pagination,
        UserListQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var usersQuery = context.Users.AsNoTracking();

        if (query.Role.HasValue)
            usersQuery = usersQuery.Where(existingUser => existingUser.Role == query.Role.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            usersQuery = usersQuery.Where(existingUser =>
                EF.Functions.ILike(existingUser.Name, pattern) ||
                EF.Functions.ILike(existingUser.Email, pattern));
        }

        return await usersQuery
            .OrderBy(existingUser => existingUser.Name)
            .Select(UserMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<UserResponseDto>> UpdateCurrentAsync(UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        user.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public Task<Result<SignedUploadUrlDto>> CreateAvatarUploadUrlAsync(
        CreateUploadUrlRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return storageService.CreateAvatarUploadUrlAsync(currentUser.UserId, request.FileExtension, cancellationToken);
    }

    public async Task<Result> DeleteCurrentAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        if (user.Role == UserRole.Admin)
        {
            var otherAdminExists = await context.Users
                .AnyAsync(existingUser => existingUser.Id != user.Id && existingUser.Role == UserRole.Admin, cancellationToken);

            if (!otherAdminExists)
                return UserErrors.LastAdmin;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserResponseDto>> AdminGetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(existingUser => existingUser.Id == userId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        return user.ToResponse();
    }

    public async Task<Result<UserResponseDto>> AdminUpdateAsync(
        Guid userId,
        AdminUpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == userId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        if (request.Email is not null)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailTaken = await context.Users
                .AnyAsync(existingUser => existingUser.Id != userId && existingUser.Email == normalizedEmail, cancellationToken);

            if (emailTaken)
                return UserErrors.EmailTaken;
        }

        var roleChanged = request.Role.HasValue && request.Role.Value != user.Role;
        if (user.Role == UserRole.Admin && request.Role == UserRole.Explorer)
        {
            var otherAdminExists = await context.Users
                .AnyAsync(existingUser => existingUser.Id != userId && existingUser.Role == UserRole.Admin, cancellationToken);

            if (!otherAdminExists)
                return UserErrors.LastAdmin;
        }

        user.ApplyAdminUpdate(request);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return UserErrors.EmailTaken;
        }

        if (roleChanged || !string.IsNullOrWhiteSpace(request.Password))
        {
            var now = timeProvider.GetUtcNow().UtcDateTime;
            await context.Sessions
                .Where(session => session.UserId == userId && session.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(session => session.RevokedAt, now), cancellationToken);
        }

        return user.ToResponse();
    }

    public async Task<Result> AdminDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == userId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        if (user.Role == UserRole.Admin)
        {
            var otherAdminExists = await context.Users
                .AnyAsync(existingUser => existingUser.Id != userId && existingUser.Role == UserRole.Admin, cancellationToken);

            if (!otherAdminExists)
                return UserErrors.LastAdmin;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
