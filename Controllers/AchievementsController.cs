using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo catálogo de conquistas do sistema.
 * Disponibiliza listagem aberta de conquistas e restringe a administração a Administradores.
 */
[ApiController]
[Route("api/achievements")]
public class AchievementsController(IAchievementService achievementService) : ApiControllerBase
{
    /**
     * Lista todas as conquistas do sistema de forma paginada.
     */
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("listAchievements")]
    [EndpointSummary("Lista todas as conquistas de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<AchievementResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<AchievementResponseDto>>> List([FromQuery] PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await achievementService.ListAsync(pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Busca detalhes de uma conquista pelo seu ID.
     */
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointName("getAchievementById")]
    [EndpointSummary("Busca uma conquista pelo ID")]
    [ProducesResponseType(typeof(AchievementResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AchievementResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await achievementService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Cria uma nova conquista no sistema (restrito a Administradores).
     */
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("createAchievement")]
    [EndpointSummary("Cadastra uma nova conquista (Administrador)")]
    [ProducesResponseType(typeof(AchievementResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AchievementResponseDto>> Create([FromBody] CreateAchievementDto request, CancellationToken cancellationToken)
    {
        var result = await achievementService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /**
     * Atualiza dados de uma conquista existente (restrito a Administradores).
     */
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("updateAchievement")]
    [EndpointSummary("Atualiza parcialmente uma conquista (Administrador)")]
    [ProducesResponseType(typeof(AchievementResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AchievementResponseDto>> Update(Guid id, [FromBody] UpdateAchievementDto request, CancellationToken cancellationToken)
    {
        var result = await achievementService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Remove uma conquista do sistema (restrito a Administradores).
     */
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("deleteAchievement")]
    [EndpointSummary("Remove uma conquista (Administrador)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await achievementService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Lista todas as conquistas disponíveis acompanhadas do progresso e status de desbloqueio do usuário logado.
     */
    [HttpGet("users/me")]
    [Authorize]
    [EndpointName("listMyAchievements")]
    [EndpointSummary("Lista todas as conquistas e o progresso do usuário autenticado")]
    [ProducesResponseType(typeof(IReadOnlyList<UserAchievementProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<UserAchievementProgressDto>>> ListMyAchievements(CancellationToken cancellationToken)
    {
        var result = await achievementService.ListMyAchievementsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Simula e estima quantos exploradores da base atingem os requisitos de uma regra antes de cadastrá-la.
     */
    [HttpGet("estimate")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("estimateAchievementImpact")]
    [EndpointSummary("Estima quantos usuários já atingem uma regra de conquista antes do cadastro")]
    [ProducesResponseType(typeof(AchievementEstimateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AchievementEstimateResponseDto>> Estimate(
        [FromQuery] AchievementEstimateQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await achievementService.EstimateAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
