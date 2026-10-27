using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Activity;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pelo registro e cálculo de métricas das atividades do explorador.
 * Gerencia validação de raio de check-in, lista de locais favoritos, consolidação estatística e selo verificado.
 */
public sealed class ActivityService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IAchievementEvaluatorService achievementEvaluatorService,
    TimeProvider timeProvider) : IActivityService
{
    private const double MaxCheckInDistanceMeters = 200.0;
    private const int RequiredVisitsForVerification = 5;
    private const int RequiredReviewsForVerification = 3;
    private const int MaxAllowedRemovedReviewsForVerification = 0;
    private const int RequiredAccountAgeDaysForVerification = 30;

    /**
     * Registra o check-in do usuário garantindo raio máximo de 200m e limite de um check-in por local ao dia.
     */
    public async Task<Result<VisitResponseDto>> CheckInAsync(
        Guid placeId,
        CheckInVisitDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var place = await context.Places
            .Include(p => p.Category)
            .SingleOrDefaultAsync(p => p.Id == placeId, cancellationToken);

        if (place is null)
            return ActivityErrors.PlaceNotFound;

        if (place.Status != PlaceStatus.Approved)
            return ActivityErrors.PlaceNotFound;

        var distanceMeters = CalculateHaversineDistanceMeters(
            request.Latitude,
            request.Longitude,
            place.Latitude,
            place.Longitude);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        var alreadyCheckedInToday = await context.Visits
            .AnyAsync(v => v.UserId == userId && v.PlaceId == placeId && v.VisitedOn == today, cancellationToken);

        if (alreadyCheckedInToday)
            return ActivityErrors.AlreadyCheckedInToday;

        var visit = new Visit
        {
            UserId = userId,
            PlaceId = placeId,
            VisitedOn = today,
            CreatedAt = now
        };

        context.Visits.Add(visit);
        await context.SaveChangesAsync(cancellationToken);

        await achievementEvaluatorService.EvaluateForUserAsync(userId, AchievementEventType.Visit, cancellationToken);

        var hasReviewed = await context.Reviews
            .AnyAsync(r => r.UserId == userId && r.PlaceId == placeId && r.RemovedAt == null, cancellationToken);

        var response = new VisitResponseDto(
            visit.Id,
            new VisitPlaceSummaryDto(
                place.Id,
                place.Name,
                place.Neighborhood,
                new CategorySummaryDto(place.Category.Id, place.Category.Name, place.Category.Slug, place.Category.Icon, place.Category.Color)
            ),
            visit.VisitedOn,
            hasReviewed,
            visit.CreatedAt
        );

        return response;
    }

    /**
     * Lista o histórico de visitas e check-ins do usuário autenticado de forma paginada.
     */
    public async Task<Result<PagedResponseDto<VisitResponseDto>>> ListMyVisitsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var query = context.Visits
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.VisitedOn)
            .ThenByDescending(v => v.CreatedAt);

        var reviewedPlaceIds = await context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RemovedAt == null)
            .Select(r => r.PlaceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reviewedSet = reviewedPlaceIds.ToHashSet();

        var total = await query.CountAsync(cancellationToken);
        var page = pagination.Page;
        var pageSize = pagination.PageSize;

        var visits = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                v.Id,
                v.VisitedOn,
                v.CreatedAt,
                PlaceId = v.Place.Id,
                PlaceName = v.Place.Name,
                PlaceNeighborhood = v.Place.Neighborhood,
                CategoryId = v.Place.Category.Id,
                CategoryName = v.Place.Category.Name,
                CategorySlug = v.Place.Category.Slug,
                CategoryIcon = v.Place.Category.Icon,
                CategoryColor = v.Place.Category.Color
            })
            .ToListAsync(cancellationToken);

        var items = visits.Select(v => new VisitResponseDto(
            v.Id,
            new VisitPlaceSummaryDto(
                v.PlaceId,
                v.PlaceName,
                v.PlaceNeighborhood,
                new CategorySummaryDto(v.CategoryId, v.CategoryName, v.CategorySlug, v.CategoryIcon, v.CategoryColor)
            ),
            v.VisitedOn,
            reviewedSet.Contains(v.PlaceId),
            v.CreatedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResponseDto<VisitResponseDto>(
            items,
            page,
            pageSize,
            total,
            totalPages
        );
    }

    /**
     * Adiciona um local aprovado aos favoritos do usuário e dispara o gatilho de conquistas.
     */
    public async Task<Result> AddFavoriteAsync(Guid placeId, CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var placeExists = await context.Places
            .AnyAsync(p => p.Id == placeId && p.Status == PlaceStatus.Approved, cancellationToken);

        if (!placeExists)
            return ActivityErrors.PlaceNotFound;

        var alreadyFavorited = await context.Favorites
            .AnyAsync(f => f.UserId == userId && f.PlaceId == placeId, cancellationToken);

        if (alreadyFavorited)
            return Result.Success();

        var favorite = new Favorite
        {
            UserId = userId,
            PlaceId = placeId,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        context.Favorites.Add(favorite);
        await context.SaveChangesAsync(cancellationToken);

        await achievementEvaluatorService.EvaluateForUserAsync(userId, AchievementEventType.Favorite, cancellationToken);

        return Result.Success();
    }

    /**
     * Remove o local da lista de favoritos do usuário.
     */
    public async Task<Result> RemoveFavoriteAsync(Guid placeId, CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var favorite = await context.Favorites
            .SingleOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId, cancellationToken);

        if (favorite is null)
            return Result.Success();

        context.Favorites.Remove(favorite);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /**
     * Lista de forma paginada os locais favoritados pelo usuário logado com métricas consolidadas em tempo de consulta.
     */
    public async Task<Result<PagedResponseDto<FavoriteResponseDto>>> ListMyFavoritesAsync(
        PaginationQueryDto pagination,
        Guid? categoryId = null,
        string? neighborhood = null,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var query = context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId);

        if (categoryId.HasValue)
            query = query.Where(f => f.Place.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(neighborhood))
            query = query.Where(f => f.Place.Neighborhood.ToLower() == neighborhood.Trim().ToLower());

        query = query.OrderByDescending(f => f.CreatedAt);

        return await query
            .Select(f => new FavoriteResponseDto(
                f.PlaceId,
                f.Place.Name,
                f.Place.Neighborhood,
                new CategorySummaryDto(f.Place.Category.Id, f.Place.Category.Name, f.Place.Category.Slug, f.Place.Category.Icon, f.Place.Category.Color),
                f.Place.Reviews.Where(r => r.RemovedAt == null).Average(r => (double?)r.Rating) ?? 0.0,
                f.Place.Reviews.Count(r => r.RemovedAt == null),
                f.Place.Photos
                    .OrderByDescending(p => p.IsCover)
                    .ThenBy(p => p.CreatedAt)
                    .Select(p => p.Url)
                    .FirstOrDefault(),
                f.CreatedAt
            ))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Consolida o perfil estatístico do usuário: totais, janela mensal de 6 meses, categorias, cobertura de bairros e notas.
     */
    public async Task<Result<UserStatsDto>> GetMyStatsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var totalVisits = await context.Visits.CountAsync(v => v.UserId == userId, cancellationToken);
        var totalFavorites = await context.Favorites.CountAsync(f => f.UserId == userId, cancellationToken);
        var totalReviews = await context.Reviews.CountAsync(r => r.UserId == userId && r.RemovedAt == null, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sixMonthsAgo = new DateOnly(now.Year, now.Month, 1).AddMonths(-5);

        var recentVisits = await context.Visits
            .AsNoTracking()
            .Where(v => v.UserId == userId && v.VisitedOn >= sixMonthsAgo)
            .Select(v => new { v.VisitedOn.Year, v.VisitedOn.Month })
            .ToListAsync(cancellationToken);

        var visitsByMonth = new List<MonthlyVisitsCountDto>();
        for (var i = 5; i >= 0; i--)
        {
            var targetMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var key = targetMonth.ToString("yyyy-MM");
            var count = recentVisits.Count(v => v.Year == targetMonth.Year && v.Month == targetMonth.Month);
            visitsByMonth.Add(new MonthlyVisitsCountDto(key, count));
        }

        /** Projeta contagens em tipo anônimo para ordenação em SQL e mapeia para o record posicional em memória. */
        var rawVisitsByCategory = await context.Visits
            .AsNoTracking()
            .Where(visit => visit.UserId == userId)
            .GroupBy(visit => new { visit.Place.CategoryId, visit.Place.Category.Name, visit.Place.Category.Color })
            .Select(group => new
            {
                group.Key.Name,
                group.Key.Color,
                Count = group.Count()
            })
            .OrderByDescending(category => category.Count)
            .ToListAsync(cancellationToken);

        var visitsByCategory = rawVisitsByCategory
            .Select(category => new CategoryVisitsDistributionDto(category.Name, category.Color, category.Count))
            .ToList();

        var visitedNeighborhoods = await context.Visits
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .Select(v => v.Place.Neighborhood.ToLower())
            .Distinct()
            .CountAsync(cancellationToken);

        var totalApprovedNeighborhoods = await context.Places
            .AsNoTracking()
            .Where(p => p.Status == PlaceStatus.Approved)
            .Select(p => p.Neighborhood.ToLower())
            .Distinct()
            .CountAsync(cancellationToken);

        var coveragePct = totalApprovedNeighborhoods == 0
            ? 0.0
            : Math.Round((visitedNeighborhoods / (double)totalApprovedNeighborhoods) * 100.0, 1);

        var neighborhoodCoverage = new NeighborhoodCoverageDto(
            visitedNeighborhoods,
            totalApprovedNeighborhoods,
            coveragePct
        );

        var userRatings = await context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RemovedAt == null)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var ratingMap = userRatings.ToDictionary(r => r.Rating, r => r.Count);
        var ratingsGiven = Enumerable.Range(1, 5)
            .Select(star => new RatingDistributionDto(star, ratingMap.GetValueOrDefault(star, 0)))
            .ToList();

        return new UserStatsDto(
            totalVisits,
            totalFavorites,
            totalReviews,
            visitsByMonth,
            visitsByCategory,
            neighborhoodCoverage,
            ratingsGiven
        );
    }

    /**
     * Avalia os 4 critérios canônicos para concessão do selo de Explorador Verificado.
     */
    public async Task<Result<UserVerificationStatusDto>> GetMyVerificationStatusAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        var visitsCount = await context.Visits.CountAsync(v => v.UserId == userId, cancellationToken);
        var publishedReviewsCount = await context.Reviews.CountAsync(r => r.UserId == userId && r.RemovedAt == null, cancellationToken);
        var removedReviewsCount = await context.Reviews.CountAsync(r => r.UserId == userId && r.RemovedAt != null, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var accountAgeDays = Math.Max(0, (int)(now - user.CreatedAt).TotalDays);

        var isVerified = visitsCount >= RequiredVisitsForVerification
            && publishedReviewsCount >= RequiredReviewsForVerification
            && removedReviewsCount <= MaxAllowedRemovedReviewsForVerification
            && accountAgeDays >= RequiredAccountAgeDaysForVerification;

        return new UserVerificationStatusDto(
            visitsCount,
            RequiredVisitsForVerification,
            publishedReviewsCount,
            RequiredReviewsForVerification,
            removedReviewsCount,
            MaxAllowedRemovedReviewsForVerification,
            accountAgeDays,
            RequiredAccountAgeDaysForVerification,
            isVerified
        );
    }

    /**
     * Calcula a distância geodésica em metros entre dois pontos geográficos usando a fórmula de Haversine.
     */
    private static double CalculateHaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6371000.0;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
