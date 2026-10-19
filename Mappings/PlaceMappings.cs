using System.Linq.Expressions;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.DTOs.Tags;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class PlaceMappings
{
    public static readonly Expression<Func<Place, PlaceSummaryDto>> ToSummaryProjection = place =>
        new PlaceSummaryDto(
            place.Id,
            place.CategoryId,
            new CategorySummaryDto(
                place.Category.Id,
                place.Category.Name,
                place.Category.Slug,
                place.Category.Icon,
                place.Category.Color
            ),
            place.Name,
            place.Latitude,
            place.Longitude,
            place.Neighborhood,
            place.About,
            place.HasWifi,
            place.IsPetFriendly,
            place.Reviews.Average(review => (double?)review.Rating) ?? 0.0,
            place.Reviews.Count,
            place.Photos
                .OrderByDescending(photo => photo.IsCover)
                .ThenBy(photo => photo.CreatedAt)
                .Select(photo => photo.Url)
                .FirstOrDefault(),
            place.PlaceTags
                .Select(placeTag => new TagResponseDto(
                    placeTag.Tag.Id,
                    placeTag.Tag.Name,
                    placeTag.Tag.Slug,
                    placeTag.Tag.CreatedAt
                ))
                .ToList(),
            place.Status,
            place.CreatedAt
        );

    public static readonly Expression<Func<Place, PlaceResponseDto>> ToResponseProjection = place =>
        new PlaceResponseDto(
            place.Id,
            place.CategoryId,
            new CategorySummaryDto(
                place.Category.Id,
                place.Category.Name,
                place.Category.Slug,
                place.Category.Icon,
                place.Category.Color
            ),
            place.Author == null ? null : new AuthorSummaryDto(place.Author.Id, place.Author.Name, place.Author.AvatarUrl),
            place.Name,
            place.Latitude,
            place.Longitude,
            place.Neighborhood,
            place.About,
            place.Description,
            place.HasWifi,
            place.IsPetFriendly,
            place.BestTime,
            place.LocalSecret,
            place.Reviews.Average(review => (double?)review.Rating) ?? 0.0,
            place.Reviews.Count,
            place.Photos
                .OrderByDescending(photo => photo.IsCover)
                .ThenBy(photo => photo.CreatedAt)
                .Select(photo => photo.Url)
                .FirstOrDefault(),
            place.Photos
                .OrderByDescending(photo => photo.IsCover)
                .ThenBy(photo => photo.CreatedAt)
                .Select(photo => new PlacePhotoResponseDto(
                    photo.Id,
                    photo.Url,
                    photo.StoragePath,
                    photo.IsCover,
                    photo.CreatedAt
                ))
                .ToList(),
            place.PlaceTags
                .Select(placeTag => new TagResponseDto(
                    placeTag.Tag.Id,
                    placeTag.Tag.Name,
                    placeTag.Tag.Slug,
                    placeTag.Tag.CreatedAt
                ))
                .ToList(),
            place.Status,
            place.CreatedAt,
            place.UpdatedAt
        );

    public static PlacePhotoResponseDto ToResponse(this PlacePhoto photo) =>
        new(
            photo.Id,
            photo.Url,
            photo.StoragePath,
            photo.IsCover,
            photo.CreatedAt
        );

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

        if (request.BestTime is not null)
            place.BestTime = string.IsNullOrWhiteSpace(request.BestTime) ? null : request.BestTime.Trim();

        if (request.LocalSecret is not null)
            place.LocalSecret = string.IsNullOrWhiteSpace(request.LocalSecret) ? null : request.LocalSecret.Trim();
    }
}
