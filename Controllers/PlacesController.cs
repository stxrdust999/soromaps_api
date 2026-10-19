using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

[ApiController]
[Route("api/places")]
public class PlacesController(IPlaceService placeService) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("listPlaces")]
    [EndpointSummary("Lista todos os lugares aprovados com suporte a filtros espaciais (bbox), categoria, tag, bairro e busca")]
    [ProducesResponseType(typeof(PagedResponseDto<PlaceSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<PlaceSummaryDto>>> List(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] PlaceListQueryDto filter,
        CancellationToken cancellationToken)
    {
        var result = await placeService.ListApprovedAsync(pagination, filter, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointName("getPlaceById")]
    [EndpointSummary("Busca os detalhes completos de um lugar pelo ID")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await placeService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost]
    [Authorize]
    [EndpointName("createPlace")]
    [EndpointSummary("Cadastra uma nova sugestão de lugar (nasce com status 'Pending')")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PlaceResponseDto>> Create([FromBody] CreatePlaceDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize]
    [EndpointName("updatePlace")]
    [EndpointSummary("Atualiza parcialmente os dados de um lugar")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> Update(Guid id, [FromBody] UpdatePlaceDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("updatePlaceStatus")]
    [EndpointSummary("Atualiza o status de aprovação de um lugar (Administrador)")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> UpdateStatus(Guid id, [FromBody] UpdatePlaceStatusDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.UpdateStatusAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [EndpointName("deletePlace")]
    [EndpointSummary("Remove um lugar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await placeService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    [HttpPost("{id:guid}/photos/upload-url")]
    [Authorize]
    [EndpointName("createPlacePhotoUploadUrl")]
    [EndpointSummary("Gera uma URL assinada para upload de foto do lugar no Supabase Storage")]
    [ProducesResponseType(typeof(SignedUploadUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SignedUploadUrlDto>> CreatePhotoUploadUrl(
        Guid id,
        [FromBody] CreateUploadUrlRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await placeService.CreatePhotoUploadUrlAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize]
    [EndpointName("addPlacePhoto")]
    [EndpointSummary("Registra uma foto enviada para o lugar")]
    [ProducesResponseType(typeof(PlacePhotoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlacePhotoResponseDto>> AddPhoto(
        Guid id,
        [FromBody] ConfirmPlacePhotoDto request,
        CancellationToken cancellationToken)
    {
        var result = await placeService.AddPhotoAsync(id, request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPatch("{id:guid}/photos/{photoId:guid}/cover")]
    [Authorize]
    [EndpointName("setPlaceCoverPhoto")]
    [EndpointSummary("Define uma foto existente como capa do lugar")]
    [ProducesResponseType(typeof(PlacePhotoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlacePhotoResponseDto>> SetCoverPhoto(
        Guid id,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var result = await placeService.SetCoverPhotoAsync(id, photoId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [Authorize]
    [EndpointName("deletePlacePhoto")]
    [EndpointSummary("Remove uma foto do lugar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(
        Guid id,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var result = await placeService.DeletePhotoAsync(id, photoId, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
