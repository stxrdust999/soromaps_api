using soromaps_api.DTOs.Activity;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para registro de atividades do usuário.
 * Engloba check-in presencial, locais favoritos, estatísticas do perfil e verificação de explorador.
 */
public interface IActivityService
{
    /**
     * Valida geolocalização e registra o check-in presencial do usuário em um local.
     */
    Task<Result<VisitResponseDto>> CheckInAsync(
        Guid placeId,
        CheckInVisitDto request,
        CancellationToken cancellationToken = default);

    /**
     * Lista o histórico paginado de check-ins do usuário autenticado.
     */
    Task<Result<PagedResponseDto<VisitResponseDto>>> ListMyVisitsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default);

    /**
     * Adiciona um local à lista de favoritos do usuário.
     */
    Task<Result> AddFavoriteAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    /**
     * Remove um local da lista de favoritos do usuário.
     */
    Task<Result> RemoveFavoriteAsync(
        Guid placeId,
        CancellationToken cancellationToken = default);

    /**
     * Lista de forma paginada os locais favoritados pelo usuário com filtros opcionais.
     */
    Task<Result<PagedResponseDto<FavoriteResponseDto>>> ListMyFavoritesAsync(
        PaginationQueryDto pagination,
        Guid? categoryId = null,
        string? neighborhood = null,
        CancellationToken cancellationToken = default);

    /**
     * Consolida métricas de atividades, visitas por período, categorias e bairros explorados.
     */
    Task<Result<UserStatsDto>> GetMyStatsAsync(CancellationToken cancellationToken = default);

    /**
     * Avalia os requisitos de elegibilidade para o selo de Explorador Verificado.
     */
    Task<Result<UserVerificationStatusDto>> GetMyVerificationStatusAsync(CancellationToken cancellationToken = default);
}
