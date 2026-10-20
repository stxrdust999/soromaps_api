using System.ComponentModel.DataAnnotations;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Feed;

public sealed record CreateFeedMuteDto(
    [Required] FeedMuteScopeType ScopeType,
    [Required, MaxLength(60)] string ScopeValue
);

public sealed record FeedMuteResponseDto(
    Guid Id,
    FeedMuteScopeType ScopeType,
    string ScopeValue,
    DateTime CreatedAt
);

public sealed record FeedQueryDto(
    string? Reason = null,
    string? Neighborhood = null,
    int? Page = 1,
    int? PageSize = 20
);

public sealed record FeedItemDto(
    string Id,
    string Kind, // "review" | "new_place" | "story" | "place_burst" | "rare_achievement"
    string Reason, // "perto" | "salvo" | "categoria" | "cidade" | "curadoria"
    string Title,
    string? Description,
    string? Neighborhood,
    Guid? PlaceId,
    string? PlaceName,
    string? CategoryName,
    string? CategoryColor,
    string? CategoryIcon,
    string? ImageUrl,
    object? Metadata,
    DateTime OccurredAt
);
