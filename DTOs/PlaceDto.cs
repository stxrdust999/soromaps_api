using soromaps_api.Models;

namespace soromaps_api.DTOs
{
    public class PlaceDto
    {
        public record PlaceResponseDto(
        Guid PlaceId,
        Guid CategoryId,
        Guid AuthorId,
        string PlaceName,
        float Lat,
        float Lng,
        string Neighborhood,
        string About,
        string Description,
        bool HasWifi,
        bool PetFriendly,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt
    )
        {
            public static PlaceResponseDto FromEntity(Place place) =>
                new(
                    place.PlaceId,
                    place.CategoryId,
                    place.AuthorId,
                    place.PlaceName,
                    place.Lat,
                    place.Lng,
                    place.Neighborhood,
                    place.About,
                    place.Description,
                    place.HasWifi,
                    place.PetFriendly,
                    place.Status.ToString(), // Converte o Enum para texto
                    place.CreatedAt,
                    place.UpdatedAt
                );
        }

        public record CreatePlaceDto(
            Guid CategoryId,
            string PlaceName,
            float Lat,
            float Lng,
            string Neighborhood,
            string About,
            string Description,
            bool HasWifi,
            bool PetFriendly
        );

        public record UpdatePlaceDto(
            Guid? CategoryId,
            string? PlaceName,
            float? Lat,
            float? Lng,
            string? Neighborhood,
            string? About,
            string? Description,
            bool? HasWifi,
            bool? PetFriendly,
            Status? Status
        );
    }
}
