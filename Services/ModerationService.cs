using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Moderation;
using soromaps_api.DTOs.Places;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class ModerationService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IModerationService
{
    private static readonly HashSet<string> AllowedReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        "duplicate",
        "incomplete_info",
        "wrong_location",
        "outside_sorocaba",
        "inappropriate",
        "does_not_exist",
        "other"
    };

    public async Task<Result<PagedResponseDto<ModerationPlaceItemDto>>> ListQueueAsync(
        PaginationQueryDto pagination,
        ModerationQueueQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var placesQuery = context.Places
            .AsNoTracking()
            .Include(place => place.Category)
            .Include(place => place.Author)
            .Include(place => place.Photos)
            .AsQueryable();

        if (query.Status.HasValue)
            placesQuery = placesQuery.Where(place => place.Status == query.Status.Value);

        if (query.CategoryId.HasValue)
            placesQuery = placesQuery.Where(place => place.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            placesQuery = placesQuery.Where(place =>
                EF.Functions.ILike(place.Name, pattern) ||
                EF.Functions.ILike(place.Neighborhood, pattern));
        }

        var totalCount = await placesQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

        var places = await placesQuery
            .OrderBy(place => place.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        var authorIds = places
            .Where(place => place.AuthorId.HasValue)
            .Select(place => place.AuthorId!.Value)
            .Distinct()
            .ToList();

        var authorStatsLookup = await context.Places
            .AsNoTracking()
            .Where(place => place.AuthorId.HasValue && authorIds.Contains(place.AuthorId.Value))
            .GroupBy(place => place.AuthorId!.Value)
            .Select(group => new
            {
                AuthorId = group.Key,
                Total = group.Count(),
                Approved = group.Count(place => place.Status == PlaceStatus.Approved)
            })
            .ToDictionaryAsync(item => item.AuthorId, cancellationToken);

        var existingPlacesForComparison = await context.Places
            .AsNoTracking()
            .Where(place => place.Status == PlaceStatus.Approved)
            .Select(place => new
            {
                place.Id,
                place.Name,
                place.Neighborhood,
                place.Latitude,
                place.Longitude,
                place.Status
            })
            .ToListAsync(cancellationToken);

        var items = places.Select(place =>
        {
            ModerationAuthorStatsDto? authorDto = null;
            if (place.Author is not null)
            {
                authorStatsLookup.TryGetValue(place.Author.Id, out var stats);
                authorDto = new ModerationAuthorStatsDto(
                    place.Author.Id,
                    place.Author.Name,
                    place.Author.AvatarUrl,
                    stats?.Total ?? 1,
                    stats?.Approved ?? 0,
                    place.Author.CreatedAt
                );
            }

            var duplicates = existingPlacesForComparison
                .Where(candidate => candidate.Id != place.Id)
                .Select(candidate => new
                {
                    Candidate = candidate,
                    DistanceMeters = CalculateDistanceMeters(place.Latitude, place.Longitude, candidate.Latitude, candidate.Longitude),
                    SameName = string.Equals(candidate.Name.Trim(), place.Name.Trim(), StringComparison.OrdinalIgnoreCase)
                })
                .Where(entry => entry.DistanceMeters <= 120.0 || entry.SameName)
                .OrderBy(entry => entry.DistanceMeters)
                .Take(3)
                .Select(entry => new DuplicatePlaceHintDto(
                    entry.Candidate.Id,
                    entry.Candidate.Name,
                    entry.Candidate.Neighborhood,
                    Math.Round(entry.DistanceMeters, 1),
                    entry.Candidate.Status
                ))
                .ToList();

            var photos = place.Photos
                .OrderByDescending(photo => photo.IsCover)
                .ThenBy(photo => photo.CreatedAt)
                .Select(photo => new PlacePhotoResponseDto(
                    photo.Id,
                    photo.Url,
                    photo.StoragePath,
                    photo.IsCover,
                    photo.CreatedAt
                ))
                .ToList();

            return new ModerationPlaceItemDto(
                place.Id,
                new CategorySummaryDto(
                    place.Category.Id,
                    place.Category.Name,
                    place.Category.Slug,
                    place.Category.Icon,
                    place.Category.Color
                ),
                authorDto,
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
                photos,
                CalculateCompletenessPercentage(place, photos.Count),
                duplicates,
                place.Status,
                place.CreatedAt,
                place.UpdatedAt
            );
        }).ToList();

        return new PagedResponseDto<ModerationPlaceItemDto>(
            items,
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages
        );
    }

    public async Task<Result<ModerationDecisionResponseDto>> DecideAsync(
        Guid placeId,
        ModerationDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Decision is not (PlaceStatus.Approved or PlaceStatus.Returned or PlaceStatus.Rejected))
            return ModerationErrors.InvalidDecision;

        if (request.Decision is PlaceStatus.Returned or PlaceStatus.Rejected)
        {
            if (string.IsNullOrWhiteSpace(request.Reason) || !AllowedReasons.Contains(request.Reason.Trim()))
                return ModerationErrors.ReasonRequired;
        }

        var place = await context.Places
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return ModerationErrors.PlaceNotFound;

        if (request.ExpectedCurrentStatus.HasValue && place.Status != request.ExpectedCurrentStatus.Value)
            return ModerationErrors.StatusConflict;

        var decisionRecord = new ModerationDecision
        {
            PlaceId = place.Id,
            ModeratorId = currentUser.UserId,
            PreviousStatus = place.Status,
            Decision = request.Decision,
            Reason = request.Decision == PlaceStatus.Approved ? null : request.Reason?.Trim().ToLowerInvariant(),
            FeedbackNote = string.IsNullOrWhiteSpace(request.FeedbackNote) ? null : request.FeedbackNote.Trim(),
            IsUndo = false
        };

        place.Status = request.Decision;
        context.ModerationDecisions.Add(decisionRecord);
        await context.SaveChangesAsync(cancellationToken);

        var moderator = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == currentUser.UserId)
            .Select(user => new AuthorSummaryDto(user.Id, user.Name, user.AvatarUrl))
            .SingleOrDefaultAsync(cancellationToken);

        return new ModerationDecisionResponseDto(
            decisionRecord.Id,
            place.Id,
            place.Name,
            moderator,
            decisionRecord.PreviousStatus,
            decisionRecord.Decision,
            decisionRecord.Reason,
            decisionRecord.FeedbackNote,
            decisionRecord.IsUndo,
            decisionRecord.CreatedAt
        );
    }

    public async Task<Result<IReadOnlyList<ModerationDecisionResponseDto>>> BatchDecideAsync(
        BatchModerationDecisionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Decision is not (PlaceStatus.Approved or PlaceStatus.Returned or PlaceStatus.Rejected))
            return ModerationErrors.InvalidDecision;

        if (request.Decision is PlaceStatus.Returned or PlaceStatus.Rejected)
        {
            if (string.IsNullOrWhiteSpace(request.Reason) || !AllowedReasons.Contains(request.Reason.Trim()))
                return ModerationErrors.ReasonRequired;
        }

        var distinctIds = request.PlaceIds.Distinct().ToList();

        var places = await context.Places
            .Where(place => distinctIds.Contains(place.Id))
            .ToListAsync(cancellationToken);

        if (places.Count != distinctIds.Count)
            return ModerationErrors.PlaceNotFound;

        var moderator = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == currentUser.UserId)
            .Select(user => new AuthorSummaryDto(user.Id, user.Name, user.AvatarUrl))
            .SingleOrDefaultAsync(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var createdDecisions = new List<(ModerationDecision Decision, string PlaceName)>(places.Count);

        foreach (var place in places)
        {
            var decisionRecord = new ModerationDecision
            {
                PlaceId = place.Id,
                ModeratorId = currentUser.UserId,
                PreviousStatus = place.Status,
                Decision = request.Decision,
                Reason = request.Decision == PlaceStatus.Approved ? null : request.Reason?.Trim().ToLowerInvariant(),
                FeedbackNote = string.IsNullOrWhiteSpace(request.FeedbackNote) ? null : request.FeedbackNote.Trim(),
                IsUndo = false
            };

            place.Status = request.Decision;
            context.ModerationDecisions.Add(decisionRecord);
            createdDecisions.Add((decisionRecord, place.Name));
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return createdDecisions
            .Select(entry => new ModerationDecisionResponseDto(
                entry.Decision.Id,
                entry.Decision.PlaceId,
                entry.PlaceName,
                moderator,
                entry.Decision.PreviousStatus,
                entry.Decision.Decision,
                entry.Decision.Reason,
                entry.Decision.FeedbackNote,
                entry.Decision.IsUndo,
                entry.Decision.CreatedAt
            ))
            .ToList();
    }

    public async Task<Result<ModerationDecisionResponseDto>> UndoAsync(
        Guid placeId,
        CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == placeId, cancellationToken);

        if (place is null)
            return ModerationErrors.PlaceNotFound;

        var lastDecision = await context.ModerationDecisions
            .AsNoTracking()
            .Where(decision => decision.PlaceId == placeId)
            .OrderByDescending(decision => decision.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastDecision is null)
            return ModerationErrors.NothingToUndo;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (now - lastDecision.CreatedAt > TimeSpan.FromHours(24))
            return ModerationErrors.UndoExpired;

        var undoRecord = new ModerationDecision
        {
            PlaceId = place.Id,
            ModeratorId = currentUser.UserId,
            PreviousStatus = place.Status,
            Decision = lastDecision.PreviousStatus,
            Reason = null,
            FeedbackNote = "Reversão de decisão anterior dentro da janela de 24h.",
            IsUndo = true
        };

        place.Status = lastDecision.PreviousStatus;
        context.ModerationDecisions.Add(undoRecord);
        await context.SaveChangesAsync(cancellationToken);

        var moderator = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == currentUser.UserId)
            .Select(user => new AuthorSummaryDto(user.Id, user.Name, user.AvatarUrl))
            .SingleOrDefaultAsync(cancellationToken);

        return new ModerationDecisionResponseDto(
            undoRecord.Id,
            place.Id,
            place.Name,
            moderator,
            undoRecord.PreviousStatus,
            undoRecord.Decision,
            undoRecord.Reason,
            undoRecord.FeedbackNote,
            undoRecord.IsUndo,
            undoRecord.CreatedAt
        );
    }

    public async Task<Result<PagedResponseDto<ModerationDecisionResponseDto>>> ListHistoryAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        return await context.ModerationDecisions
            .AsNoTracking()
            .OrderByDescending(decision => decision.CreatedAt)
            .Select(decision => new ModerationDecisionResponseDto(
                decision.Id,
                decision.PlaceId,
                decision.Place.Name,
                decision.Moderator == null
                    ? null
                    : new AuthorSummaryDto(decision.Moderator.Id, decision.Moderator.Name, decision.Moderator.AvatarUrl),
                decision.PreviousStatus,
                decision.Decision,
                decision.Reason,
                decision.FeedbackNote,
                decision.IsUndo,
                decision.CreatedAt
            ))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    private static int CalculateCompletenessPercentage(Place place, int photosCount)
    {
        var score = 50; // Campos obrigatórios (nome, coordenadas, bairro, about, descrição)
        if (photosCount > 0)
            score += 20;
        if (!string.IsNullOrWhiteSpace(place.BestTime))
            score += 15;
        if (!string.IsNullOrWhiteSpace(place.LocalSecret))
            score += 15;

        return score;
    }

    private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000.0;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
}
