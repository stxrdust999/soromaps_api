using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Places;

public sealed record CreatePlaceDto(
    [Required(ErrorMessage = "place.category_id_required")]
    Guid CategoryId,

    [Required(ErrorMessage = "place.name_required")]
    [MaxLength(120, ErrorMessage = "place.name_too_long")]
    string Name,

    [Range(-90.0, 90.0, ErrorMessage = "place.invalid_latitude")]
    double Latitude,

    [Range(-180.0, 180.0, ErrorMessage = "place.invalid_longitude")]
    double Longitude,

    [Required(ErrorMessage = "place.neighborhood_required")]
    [MaxLength(60, ErrorMessage = "place.neighborhood_too_long")]
    string Neighborhood,

    [Required(ErrorMessage = "place.about_required")]
    [MaxLength(280, ErrorMessage = "place.about_too_long")]
    string About,

    [Required(ErrorMessage = "place.description_required")]
    [MaxLength(2000, ErrorMessage = "place.description_too_long")]
    string Description,

    bool HasWifi,

    bool IsPetFriendly
);
