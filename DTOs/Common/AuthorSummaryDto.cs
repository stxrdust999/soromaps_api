namespace soromaps_api.DTOs.Common;

public sealed record AuthorSummaryDto(
    Guid Id,
    string Name,
    string? AvatarUrl
);
