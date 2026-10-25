using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Register)]
    [EndpointName("registerUser")]
    [EndpointSummary("Cadastra um novo usuário")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterUserDto request, CancellationToken cancellationToken)
    {
        var result = await userService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetPublicProfile), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("me")]
    [Authorize]
    [EndpointName("getCurrentUser")]
    [EndpointSummary("Retorna os dados do perfil do usuário autenticado")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await userService.GetCurrentAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPatch("me")]
    [Authorize]
    [EndpointName("updateCurrentUser")]
    [EndpointSummary("Atualiza os dados do perfil do usuário autenticado")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> UpdateCurrent([FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var result = await userService.UpdateCurrentAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /** Altera a senha do usuário autenticado mediante validação da senha atual. */
    [HttpPost("me/change-password")]
    [Authorize]
    [EndpointName("changePassword")]
    [EndpointSummary("Altera a senha do usuário autenticado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
    {
        var result = await userService.ChangePasswordAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /** Altera o e-mail do usuário autenticado mediante confirmação de senha. */
    [HttpPatch("me/email")]
    [Authorize]
    [EndpointName("changeEmail")]
    [EndpointSummary("Altera o e-mail do usuário autenticado")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> ChangeEmail([FromBody] ChangeEmailDto request, CancellationToken cancellationToken)
    {
        var result = await userService.ChangeEmailAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost("me/avatar/upload-url")]
    [Authorize]
    [EndpointName("createAvatarUploadUrl")]
    [EndpointSummary("Gera uma URL assinada para upload de foto de perfil no Supabase Storage")]
    [ProducesResponseType(typeof(SignedUploadUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SignedUploadUrlDto>> CreateAvatarUploadUrl(
        [FromBody] CreateUploadUrlRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateAvatarUploadUrlAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpDelete("me")]
    [Authorize]
    [EndpointName("deleteCurrentUser")]
    [EndpointSummary("Exclui a conta do usuário autenticado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCurrent(CancellationToken cancellationToken)
    {
        var result = await userService.DeleteCurrentAsync(cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointName("getPublicUserProfile")]
    [EndpointSummary("Busca o perfil público de um usuário pela ID")]
    [ProducesResponseType(typeof(PublicUserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicUserProfileDto>> GetPublicProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetPublicProfileAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("listUsers")]
    [EndpointSummary("Lista usuários com paginação e filtros (Administrador)")]
    [ProducesResponseType(typeof(PagedResponseDto<UserResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<UserResponseDto>>> List(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] UserListQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await userService.ListAsync(pagination, query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpGet("{id:guid}/admin")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("adminGetUser")]
    [EndpointSummary("Busca os dados completos de um usuário pelo ID (Administrador)")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponseDto>> AdminGetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.AdminGetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("adminUpdateUser")]
    [EndpointSummary("Atualiza dados ou papel de um usuário pelo ID (Administrador)")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponseDto>> AdminUpdate(
        Guid id,
        [FromBody] AdminUpdateUserDto request,
        CancellationToken cancellationToken)
    {
        var result = await userService.AdminUpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("adminDeleteUser")]
    [EndpointSummary("Remove um usuário pelo ID (Administrador)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdminDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.AdminDeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
