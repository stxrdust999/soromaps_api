namespace soromaps_api.Tests.Services;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using soromaps_api.DTOs.Auth;
using soromaps_api.Errors;
using soromaps_api.Models;
using soromaps_api.Options;
using soromaps_api.Services;
using soromaps_api.Services.Interfaces;
using soromaps_api.Tests.Common;

/**
 * Suíte de testes unitários para AuthService utilizando DbContext em memória e mocks de serviços adjacentes.
 */
public sealed class AuthServiceTests
{
    private readonly ITokenService tokenService = Substitute.For<ITokenService>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));

    private readonly IOptions<JwtOptions> jwtOptions = Options.Create(new JwtOptions
    {
        Key = "super_secret_test_key_with_at_least_32_characters!",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 7
    });

    private readonly IOptions<AuthOptions> authOptions = Options.Create(new AuthOptions());

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldReturnInvalidCredentials()
    {
        using var context = TestDbContextFactory.Create();
        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new LoginRequestDto("inexistente@soromaps.com", "QualquerSenha123!");

        var result = await sut.LoginAsync(request, "TestBrowser");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ShouldReturnInvalidCredentials()
    {
        using var context = TestDbContextFactory.Create();
        var password = "CorrectPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        var user = TestUserFactory.CreateUser(email: "valid@soromaps.com", passwordHash: passwordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new LoginRequestDto("valid@soromaps.com", "WrongPassword123!");

        var result = await sut.LoginAsync(request, "TestBrowser");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(AuthErrors.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldPersistSessionAndReturnTokens()
    {
        using var context = TestDbContextFactory.Create();
        var password = "CorrectPassword123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        var user = TestUserFactory.CreateUser(email: "valid@soromaps.com", passwordHash: passwordHash);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("mocked-jwt-token");
        tokenService.GenerateRefreshToken().Returns("mocked-raw-refresh-token");
        tokenService.HashToken("mocked-raw-refresh-token").Returns("mocked-token-hash");

        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new LoginRequestDto("valid@soromaps.com", password);

        var result = await sut.LoginAsync(request, "TestBrowser");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("mocked-jwt-token");
        result.Value.RefreshToken.Should().Be("mocked-raw-refresh-token");

        var persistedSession = await context.Sessions.SingleOrDefaultAsync(s => s.UserId == user.Id);
        persistedSession.Should().NotBeNull();
        persistedSession!.TokenHash.Should().Be("mocked-token-hash");
    }

    [Fact]
    public async Task RefreshAsync_WhenSessionDoesNotExist_ShouldReturnInvalidRefreshToken()
    {
        using var context = TestDbContextFactory.Create();
        tokenService.HashToken("invalid-raw-token").Returns("non-existent-hash");

        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new RefreshTokenRequestDto("invalid-raw-token");

        var result = await sut.RefreshAsync(request, "TestBrowser");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_WhenSessionIsValid_ShouldRotateTokensAndRevokeOldSession()
    {
        using var context = TestDbContextFactory.Create();
        var user = TestUserFactory.CreateUser();
        context.Users.Add(user);

        var oldSession = new Session
        {
            UserId = user.Id,
            TokenHash = "old-token-hash",
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(7),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        context.Sessions.Add(oldSession);
        await context.SaveChangesAsync();

        tokenService.HashToken("old-raw-token").Returns("old-token-hash");
        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("new-jwt-token");
        tokenService.GenerateRefreshToken().Returns("new-raw-token");
        tokenService.HashToken("new-raw-token").Returns("new-token-hash");

        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new RefreshTokenRequestDto("old-raw-token");

        var result = await sut.RefreshAsync(request, "TestBrowser");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-jwt-token");
        result.Value.RefreshToken.Should().Be("new-raw-token");

        oldSession.RevokedAt.Should().Be(timeProvider.GetUtcNow().UtcDateTime);

        var newSession = await context.Sessions.SingleOrDefaultAsync(s => s.TokenHash == "new-token-hash");
        newSession.Should().NotBeNull();
        newSession!.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task LogoutAsync_WhenSessionExistsAndBelongsToCurrentUser_ShouldRevokeSession()
    {
        using var context = TestDbContextFactory.Create();
        var user = TestUserFactory.CreateUser();
        context.Users.Add(user);

        var session = new Session
        {
            UserId = user.Id,
            TokenHash = "active-token-hash",
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(7),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        currentUser.UserId.Returns(user.Id);
        tokenService.HashToken("active-raw-token").Returns("active-token-hash");

        var sut = new AuthService(context, tokenService, currentUser, jwtOptions, authOptions, timeProvider);
        var request = new RefreshTokenRequestDto("active-raw-token");

        var result = await sut.LogoutAsync(request);

        result.IsSuccess.Should().BeTrue();
        session.RevokedAt.Should().Be(timeProvider.GetUtcNow().UtcDateTime);
    }
}
