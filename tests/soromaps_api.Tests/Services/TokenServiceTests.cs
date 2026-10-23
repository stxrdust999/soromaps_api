namespace soromaps_api.Tests.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using soromaps_api.Models.Enums;
using soromaps_api.Options;
using soromaps_api.Services;
using soromaps_api.Tests.Common;

/**
 * Suíte de testes unitários para emissão e hashing de tokens criptográficos.
 */
public sealed class TokenServiceTests
{
    private readonly JwtOptions jwtOptions = new()
    {
        Key = "super_secret_test_key_with_at_least_32_characters!",
        Issuer = "soromaps_api_test",
        Audience = "soromaps_web_test",
        AccessTokenLifetimeMinutes = 15,
        RefreshTokenLifetimeDays = 7
    };

    [Fact]
    public void GenerateAccessToken_ShouldIncludeRequiredClaimsAndHonorExpirationWindow()
    {
        var fixedTime = DateTimeOffset.UtcNow;
        var fakeTimeProvider = new FakeTimeProvider(fixedTime);
        var sut = new TokenService(Options.Create(jwtOptions), fakeTimeProvider);
        var user = TestUserFactory.CreateUser(role: UserRole.Explorer);

        var tokenString = sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be(jwtOptions.Issuer);
        token.Audiences.Should().Contain(jwtOptions.Audience);

        var subClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub");
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());

        var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email");
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);

        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("explorer");

        var expectedExpiration = fixedTime.UtcDateTime.AddMinutes(jwtOptions.AccessTokenLifetimeMinutes);
        token.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceNonEmptyBase64String()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var sut = new TokenService(Options.Create(jwtOptions), fakeTimeProvider);

        var token1 = sut.GenerateRefreshToken();
        var token2 = sut.GenerateRefreshToken();

        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void HashToken_ShouldGenerateDeterministicLowerCaseHexSha256()
    {
        var fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var sut = new TokenService(Options.Create(jwtOptions), fakeTimeProvider);
        var rawToken = "sample-secret-refresh-token";

        var hash1 = sut.HashToken(rawToken);
        var hash2 = sut.HashToken(rawToken);

        hash1.Should().Be(hash2);
        hash1.Length.Should().Be(64);
        hash1.Should().MatchRegex("^[a-f0-9]{64}$");
    }
}
