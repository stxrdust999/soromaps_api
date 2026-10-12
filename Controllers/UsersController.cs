using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pela gestão de usuários e perfis.
 * Separa explicitamente rotas públicas, rotas do usuário logado (/me) e administração.
 */
public class UsersController(IUserService userService) : ApiControllerBase
{
    /**
     * Cadastra um novo usuário no sistema com rate limiting de prevenção contra abuso.
     */
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    [EndpointSummary("Cadastra um novo usuário")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserDto request, CancellationToken cancellationToken)
    {
        var result = await userService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetPublicProfile), new { id = result.Value.Id }, result.Value);
    }

    /**
     * Retorna o perfil completo do usuário autenticado no token atual.
     */
    [HttpGet("me")]
    [Authorize]
    [EndpointSummary("Retorna os dados do perfil do usuário autenticado")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await userService.GetCurrentAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Atualiza dados de perfil do usuário autenticado sem risco de IDOR.
     */
    [HttpPatch("me")]
    [Authorize]
    [EndpointSummary("Atualiza os dados do perfil do usuário autenticado")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> UpdateCurrent([FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateCurrentAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Exclui a conta do usuário autenticado.
     */
    [HttpDelete("me")]
    [Authorize]
    [EndpointSummary("Exclui a conta do usuário autenticado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCurrent(CancellationToken cancellationToken)
    {
        var result = await userService.DeleteCurrentAsync(cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Retorna dados públicos de um usuário por ID omitindo e-mail para privacidade.
     */
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Busca o perfil público de um usuário pela ID")]
    [ProducesResponseType(typeof(PublicUserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicUserProfileDto>> GetPublicProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetPublicProfileAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Lista usuários do sistema com paginação (exclusivo para Administradores).
     */
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointSummary("Lista usuários com paginação (Administrador)")]
    [ProducesResponseType(typeof(PagedResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<UserResponseDto>>> List([FromQuery] PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await userService.ListAsync(pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
