using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Places;

public sealed record UpdatePlaceDto(
    Guid? CategoryId = null,

    [MaxLength(120, ErrorMessage = "place.name_too_long")]
    string? Name = null,

    [Range(-90.0, 90.0, ErrorMessage = "place.invalid_latitude")]
    double? Latitude = null,

    [Range(-180.0, 180.0, ErrorMessage = "place.invalid_longitude")]
    double? Longitude = null,

    [MaxLength(60, ErrorMessage = "place.neighborhood_too_long")]
    string? Neighborhood = null,

    [MaxLength(280, ErrorMessage = "place.about_too_long")]
    string? About = null,

    [MaxLength(2000, ErrorMessage = "place.description_too_long")]
    string? Description = null,

    bool? HasWifi = null,

    bool? IsPetFriendly = null,

    [MaxLength(100, ErrorMessage = "place.best_time_too_long")]
    string? BestTime = null,

    [MaxLength(300, ErrorMessage = "place.local_secret_too_long")]
    string? LocalSecret = null,

    IReadOnlyList<Guid>? TagIds = null
);
