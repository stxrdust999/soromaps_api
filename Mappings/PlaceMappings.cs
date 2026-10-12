using System.Linq.Expressions;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre entidade Place e seus respectivos DTOs.
 * Fornece expressões de projeção para o EF Core e métodos de extensão para objetos em memória.
 */
public static class PlaceMappings
{
    /**
     * Expressão de projeção de resumo para listagens de mapa.
     * Gera uma query SQL trazendo apenas as colunas estritamente necessárias para plotar marcadores,
     * economizando memória do servidor e tráfego de rede ao ignorar descrições longas.
     */
    public static readonly Expression<Func<Place, PlaceSummaryDto>> ToSummaryProjection = place =>
        new PlaceSummaryDto(
            place.Id,
            place.CategoryId,
            place.Name,
            place.Latitude,
            place.Longitude,
            place.Neighborhood,
            place.HasWifi,
            place.IsPetFriendly,
            place.Status,
            place.CreatedAt
        );

    /**
     * Expressão de projeção completa para consulta de detalhes de local.
     */
    public static readonly Expression<Func<Place, PlaceResponseDto>> ToResponseProjection = place =>
        new PlaceResponseDto(
            place.Id,
            place.CategoryId,
            place.Author == null ? null : new AuthorSummaryDto(place.Author.Id, place.Author.Name, place.Author.AvatarUrl),
            place.Name,
            place.Latitude,
            place.Longitude,
            place.Neighborhood,
            place.About,
            place.Description,
            place.HasWifi,
            place.IsPetFriendly,
            place.Status,
            place.CreatedAt,
            place.UpdatedAt
        );

    /**
     * Mapeia uma entidade Place carregada em memória para o seu DTO de resposta detalhado.
     */
    public static PlaceResponseDto ToResponse(this Place place) =>
        new(
            place.Id,
            place.CategoryId,
            place.Author == null ? null : new AuthorSummaryDto(place.Author.Id, place.Author.Name, place.Author.AvatarUrl),
            place.Name,
            place.Latitude,
            place.Longitude,
            place.Neighborhood,
            place.About,
            place.Description,
            place.HasWifi,
            place.IsPetFriendly,
            place.Status,
            place.CreatedAt,
            place.UpdatedAt
        );

    /**
     * Aplica alterações parciais vindas de um DTO de update, ignorando campos nulos.
     */
    public static void ApplyUpdate(this Place place, UpdatePlaceDto request)
    {
        if (request.CategoryId.HasValue)
            place.CategoryId = request.CategoryId.Value;

        if (request.Name is not null)
            place.Name = request.Name.Trim();

        if (request.Latitude.HasValue)
            place.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            place.Longitude = request.Longitude.Value;

        if (request.Neighborhood is not null)
            place.Neighborhood = request.Neighborhood.Trim();

        if (request.About is not null)
            place.About = request.About.Trim();

        if (request.Description is not null)
            place.Description = request.Description.Trim();

        if (request.HasWifi.HasValue)
            place.HasWifi = request.HasWifi.Value;

        if (request.IsPetFriendly.HasValue)
            place.IsPetFriendly = request.IsPetFriendly.Value;
    }
}
