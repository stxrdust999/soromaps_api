using soromaps_api.DTOs.Common;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Places;

public sealed record PlaceResponseDto(
    Guid Id,
    Guid CategoryId,
    AuthorSummaryDto? Author,
    string Name,
    double Latitude,
    double Longitude,
    string Neighborhood,
    string About,
    string Description,
    bool HasWifi,
    bool IsPetFriendly,
    PlaceStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
