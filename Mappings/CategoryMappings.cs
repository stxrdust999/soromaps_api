using System.Linq.Expressions;
using soromaps_api.DTOs.Categories;
using soromaps_api.Extensions;
using soromaps_api.Models;
using soromaps_api.Models.Enums;

namespace soromaps_api.Mappings;

public static class CategoryMappings
{
    public static Expression<Func<Category, CategoryResponseDto>> ToResponseProjection(DateTime oneWeekAgoUtc) =>
        category => new CategoryResponseDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Icon,
            category.Color,
            category.SortOrder,
            category.IsActive,
            category.Places.Count(place => place.Status == PlaceStatus.Approved),
            category.Places.Count(place => place.Status == PlaceStatus.Approved && place.CreatedAt >= oneWeekAgoUtc),
            category.CreatedAt,
            category.UpdatedAt
        );

    public static CategorySummaryDto ToSummary(this Category category) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.Icon,
            category.Color
        );

    public static CategoryResponseDto ToResponse(this Category category, int placesCount = 0, int newPlacesThisWeekCount = 0) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.Icon,
            category.Color,
            category.SortOrder,
            category.IsActive,
            placesCount,
            newPlacesThisWeekCount,
            category.CreatedAt,
            category.UpdatedAt
        );

    public static void ApplyUpdate(this Category category, UpdateCategoryDto request)
    {
        if (request.Name is not null)
        {
            category.Name = request.Name.Trim();
            if (request.Slug is null)
                category.Slug = SlugHelper.GenerateSlug(category.Name);
        }

        if (request.Slug is not null)
            category.Slug = SlugHelper.GenerateSlug(request.Slug);

        if (request.Icon is not null)
            category.Icon = request.Icon.Trim();

        if (request.Color is not null)
            category.Color = request.Color.Trim().ToUpperInvariant();

        if (request.SortOrder.HasValue)
            category.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;
    }
}
