using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Options;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class PlaceService(
    AppDbContext context,
    ICurrentUserService currentUser,
    ISupabaseStorageService storageService,
    IOptions<SupabaseOptions> supabaseOptions) : IPlaceService
{
    public async Task<Result<PagedResponseDto<PlaceSummaryDto>>> ListApprovedAsync(
        PaginationQueryDto pagination,
        PlaceListQueryDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = context.Places
            .AsNoTracking()
            .Where(place => place.Status == PlaceStatus.Approved);

        if (filter.CategoryId.HasValue)
            query = query.Where(place => place.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var categorySlug = filter.CategorySlug.Trim().ToLowerInvariant();
            query = query.Where(place => place.Category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.TagSlug))
        {
            var tagSlug = filter.TagSlug.Trim().ToLowerInvariant();
            query = query.Where(place => place.PlaceTags.Any(placeTag => placeTag.Tag.Slug == tagSlug));
        }

        if (!string.IsNullOrWhiteSpace(filter.Neighborhood))
        {
            var neighborhood = filter.Neighborhood.Trim();
            query = query.Where(place => EF.Functions.ILike(place.Neighborhood, neighborhood));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(place =>
                EF.Functions.ILike(place.Name, pattern) ||
                EF.Functions.ILike(place.About, pattern) ||
                EF.Functions.ILike(place.Neighborhood, pattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Bbox))
        {
            if (!TryParseBoundingBox(filter.Bbox, out var minLng, out var minLat, out var maxLng, out var maxLat))
                return PlaceErrors.InvalidBoundingBox;

            query = query.Where(place =>
                place.Longitude >= minLng &&
                place.Longitude <= maxLng &&
                place.Latitude >= minLat &&
                place.Latitude <= maxLat);
        }

        var sortedQuery = (filter.Sort?.ToLowerInvariant()) switch
        {
            "rating" => query
                .OrderByDescending(place => place.Reviews.Average(review => (double?)review.Rating) ?? 0.0)
                .ThenByDescending(place => place.Reviews.Count),
            "reviews" => query
                .OrderByDescending(place => place.Reviews.Count)
                .ThenByDescending(place => place.CreatedAt),
            _ => query.OrderByDescending(place => place.CreatedAt)
        };

        return await sortedQuery
            .Select(PlaceMappings.ToSummaryProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<PlaceResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var placeMeta = await context.Places
            .AsNoTracking()
            .Where(existingPlace => existingPlace.Id == id)
            .Select(existingPlace => new { existingPlace.Id, existingPlace.Status, existingPlace.AuthorId })
            .SingleOrDefaultAsync(cancellationToken);

        if (placeMeta is null)
            return PlaceErrors.NotFound;

        var isAuthor = currentUser.IsAuthenticated && placeMeta.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (placeMeta.Status != PlaceStatus.Approved && !isAuthor && !isAdmin)
            return PlaceErrors.NotFound;

        var response = await context.Places
            .AsNoTracking()
            .Where(existingPlace => existingPlace.Id == id)
            .Select(PlaceMappings.ToResponseProjection)
            .SingleOrDefaultAsync(cancellationToken);

        if (response is null)
            return PlaceErrors.NotFound;

        return response;
    }

    public async Task<Result<PlaceResponseDto>> CreateAsync(CreatePlaceDto request, CancellationToken cancellationToken = default)
    {
        var categoryExists = await context.Categories
            .AnyAsync(category => category.Id == request.CategoryId && category.IsActive, cancellationToken);

        if (!categoryExists)
            return PlaceErrors.CategoryNotFound;

        var place = new Place
        {
            CategoryId = request.CategoryId,
            AuthorId = currentUser.UserId,
            Name = request.Name.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Neighborhood = request.Neighborhood.Trim(),
            About = request.About.Trim(),
            Description = request.Description.Trim(),
            HasWifi = request.HasWifi,
            IsPetFriendly = request.IsPetFriendly,
            BestTime = string.IsNullOrWhiteSpace(request.BestTime) ? null : request.BestTime.Trim(),
            LocalSecret = string.IsNullOrWhiteSpace(request.LocalSecret) ? null : request.LocalSecret.Trim(),
            Status = PlaceStatus.Pending
        };

        context.Places.Add(place);
        await context.SaveChangesAsync(cancellationToken);

        if (request.TagIds is { Count: > 0 })
        {
            var validTagIds = await context.Tags
                .Where(tag => request.TagIds.Contains(tag.Id))
                .Select(tag => tag.Id)
                .ToListAsync(cancellationToken);

            foreach (var tagId in validTagIds.Distinct())
            {
                context.PlaceTags.Add(new PlaceTag
                {
                    PlaceId = place.Id,
                    TagId = tagId
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(place.Id, cancellationToken);
    }

    public async Task<Result<PlaceResponseDto>> UpdateAsync(Guid id, UpdatePlaceDto request, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.PlaceTags)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await context.Categories
                .AnyAsync(category => category.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
                return PlaceErrors.CategoryNotFound;
        }

        place.ApplyUpdate(request);

        if (isOwner && !isAdmin && place.Status == PlaceStatus.Returned)
            place.Status = PlaceStatus.Pending;

        if (request.TagIds is not null)
        {
            var validTagIds = await context.Tags
                .Where(tag => request.TagIds.Contains(tag.Id))
                .Select(tag => tag.Id)
                .ToListAsync(cancellationToken);

            context.PlaceTags.RemoveRange(place.PlaceTags);

            foreach (var tagId in validTagIds.Distinct())
            {
                context.PlaceTags.Add(new PlaceTag
                {
                    PlaceId = place.Id,
                    TagId = tagId
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(place.Id, cancellationToken);
    }

    public async Task<Result<PlaceResponseDto>> UpdateStatusAsync(Guid id, UpdatePlaceStatusDto request, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var previousStatus = place.Status;
        place.Status = request.Status;

        context.ModerationDecisions.Add(new ModerationDecision
        {
            PlaceId = place.Id,
            ModeratorId = currentUser.UserId,
            PreviousStatus = previousStatus,
            Decision = request.Status
        });

        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(place.Id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.Photos)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        var storagePathsToDelete = place.Photos
            .Where(photo => !string.IsNullOrWhiteSpace(photo.StoragePath))
            .Select(photo => photo.StoragePath!)
            .ToList();

        context.Places.Remove(place);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var storagePath in storagePathsToDelete)
            await storageService.DeleteObjectAsync(supabaseOptions.Value.PlacesBucket, storagePath, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<SignedUploadUrlDto>> CreatePhotoUploadUrlAsync(
        Guid placeId,
        CreateUploadUrlRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .AsNoTracking()
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        return await storageService.CreatePlacePhotoUploadUrlAsync(placeId, request.FileExtension, cancellationToken);
    }

    public async Task<Result<PlacePhotoResponseDto>> AddPhotoAsync(
        Guid placeId,
        ConfirmPlacePhotoDto request,
        CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.Photos)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        var shouldBeCover = request.IsCover || place.Photos.Count == 0;

        if (shouldBeCover)
        {
            foreach (var existingPhoto in place.Photos.Where(photo => photo.IsCover))
                existingPhoto.IsCover = false;
        }

        var photo = new PlacePhoto
        {
            PlaceId = placeId,
            UploadedById = currentUser.UserId,
            Url = request.Url.Trim(),
            StoragePath = string.IsNullOrWhiteSpace(request.StoragePath) ? null : request.StoragePath.Trim(),
            IsCover = shouldBeCover
        };

        context.PlacePhotos.Add(photo);
        await context.SaveChangesAsync(cancellationToken);

        return photo.ToResponse();
    }

    public async Task<Result<PlacePhotoResponseDto>> SetCoverPhotoAsync(
        Guid placeId,
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.Photos)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        var targetPhoto = place.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (targetPhoto is null)
            return PlaceErrors.PhotoNotFound;

        foreach (var photo in place.Photos)
            photo.IsCover = photo.Id == photoId;

        await context.SaveChangesAsync(cancellationToken);

        return targetPhoto.ToResponse();
    }

    public async Task<Result> DeletePhotoAsync(
        Guid placeId,
        Guid photoId,
        CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.Photos)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        var targetPhoto = place.Photos.FirstOrDefault(photo => photo.Id == photoId);
        if (targetPhoto is null)
            return PlaceErrors.PhotoNotFound;

        var wasCover = targetPhoto.IsCover;
        var storagePath = targetPhoto.StoragePath;

        context.PlacePhotos.Remove(targetPhoto);

        if (wasCover)
        {
            var nextCover = place.Photos
                .Where(photo => photo.Id != photoId)
                .OrderBy(photo => photo.CreatedAt)
                .FirstOrDefault();

            if (nextCover is not null)
                nextCover.IsCover = true;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storagePath))
            await storageService.DeleteObjectAsync(supabaseOptions.Value.PlacesBucket, storagePath, cancellationToken);

        return Result.Success();
    }

    private static bool TryParseBoundingBox(
        string bbox,
        out double minLng,
        out double minLat,
        out double maxLng,
        out double maxLat)
    {
        minLng = minLat = maxLng = maxLat = 0;

        var parts = bbox.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
            return false;

        if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out minLng) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out minLat) ||
            !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out maxLng) ||
            !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out maxLat))
        {
            return false;
        }

        return minLng <= maxLng && minLat <= maxLat;
    }
}
