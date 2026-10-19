using System.Linq.Expressions;
using soromaps_api.DTOs.Tags;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class TagMappings
{
    public static readonly Expression<Func<Tag, TagResponseDto>> ToResponseProjection = tag =>
        new TagResponseDto(
            tag.Id,
            tag.Name,
            tag.Slug,
            tag.CreatedAt
        );

    public static TagResponseDto ToResponse(this Tag tag) =>
        new(
            tag.Id,
            tag.Name,
            tag.Slug,
            tag.CreatedAt
        );
}
