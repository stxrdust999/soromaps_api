namespace soromaps_api.Tests.Controllers;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using soromaps_api.Controllers;
using soromaps_api.DTOs.Auth;
using soromaps_api.DTOs.Users;
using soromaps_api.Errors;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

/**
 * Suíte de testes unitários para o AuthController validando respostas HTTP e tradução de erros.
 */
public sealed class AuthControllerTests
{
    private readonly IAuthService authService = Substitute.For<IAuthService>();

    private AuthController CreateController()
    {
        var controller = new AuthController(authService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.UserAgent = "TestUserAgent/1.0";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnOkWithAuthResponse()
    {
        var controller = CreateController();
        var request = new LoginRequestDto("explorer@soromaps.com", "Password123!");
        var userResponse = new UserResponseDto(
            Guid.NewGuid(),
            "Explorer",
            "explorer@soromaps.com",
            "explorer",
            null,
            null,
            "Campolim",
            DateTime.UtcNow
        );
        var authResponse = new AuthResponseDto("access-token-jwt", "refresh-token-raw", 900, userResponse);

        authService.LoginAsync(request, "TestUserAgent/1.0", Arg.Any<CancellationToken>())
            .Returns(Result<AuthResponseDto>.Success(authResponse));

        var actionResult = await controller.Login(request, CancellationToken.None);

        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(authResponse);
    }

    [Fact]
    public async Task Login_WhenServiceReturnsInvalidCredentialsError_ShouldReturnUnauthorizedProblemDetails()
    {
        var controller = CreateController();
        var request = new LoginRequestDto("explorer@soromaps.com", "WrongPassword!");

        authService.LoginAsync(request, "TestUserAgent/1.0", Arg.Any<CancellationToken>())
            .Returns(Result<AuthResponseDto>.Failure(AuthErrors.InvalidCredentials));

        var actionResult = await controller.Login(request, CancellationToken.None);

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be(AuthErrors.InvalidCredentials.Message);
        problemDetails.Extensions["code"].Should().Be(AuthErrors.InvalidCredentials.Code);
    }
}
