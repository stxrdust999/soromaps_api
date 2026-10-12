using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Places;

public sealed record PlaceSummaryDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    double Latitude,
    double Longitude,
    string Neighborhood,
    bool HasWifi,
    bool IsPetFriendly,
    PlaceStatus Status,
    DateTime CreatedAt
);
