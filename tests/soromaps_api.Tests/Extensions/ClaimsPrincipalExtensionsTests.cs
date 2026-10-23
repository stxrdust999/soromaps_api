namespace soromaps_api.Tests.Extensions;

using System.Security.Claims;
using FluentAssertions;
using soromaps_api.Extensions;

/**
 * Suíte de testes unitários para os métodos de extensão sobre ClaimsPrincipal.
 */
public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WhenNameIdentifierClaimExistsAndIsValidGuid_ShouldReturnParsedGuid()
    {
        var expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = principal.GetUserId();

        result.Should().Be(expectedUserId);
    }

    [Theory]
    [InlineData("not-a-valid-guid")]
    [InlineData("")]
    public void GetUserId_WhenNameIdentifierClaimIsInvalid_ShouldReturnEmptyGuid(string invalidGuidString)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, invalidGuidString)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetUserId();

        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GetUserId_WhenClaimIsMissing_ShouldReturnEmptyGuid()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = principal.GetUserId();

        result.Should().Be(Guid.Empty);
    }

    [Theory]
    [InlineData("admin", true)]
    [InlineData("Admin", false)]
    [InlineData("explorer", false)]
    public void IsAdmin_ShouldIdentifyAdminRoleAccurately(string roleName, bool expectedIsAdmin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, roleName)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var result = principal.IsAdmin();

        result.Should().Be(expectedIsAdmin);
    }
}
