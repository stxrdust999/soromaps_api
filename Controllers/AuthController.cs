using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using soromaps_api.Constants;
using soromaps_api.DTOs.Auth;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelos fluxos de autenticação e gerenciamento de sessões.
 * Gerencia login com rate limiting, renovação de tokens (RTR) e encerramento de sessões (logout).
 */
public class AuthController(IAuthService authService) : ApiControllerBase
{
    /**
     * Realiza login por e-mail e senha.
     * Retorna o access token JWT de curta duração e o refresh token de rotação.
     */
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        /** Captura o User-Agent da requisição para auditoria da sessão. */
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await authService.LoginAsync(request, userAgent, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Renova o par de credenciais apresentando um refresh token válido.
     */
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Refresh)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var result = await authService.RefreshAsync(request, userAgent, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Invalida a sessão ativa do dispositivo correspondente ao refresh token informado.
     */
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Invalida todas as sessões ativas do usuário autenticado em todos os aparelhos.
     */
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAllAsync(cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
