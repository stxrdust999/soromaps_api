using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Community;
using soromaps_api.DTOs.Places;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Errors;
using soromaps_api.Mappings;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por consultas públicas da comunidade de exploradores e perfis.
 */
public sealed class CommunityService(
    AppDbContext context,
    TimeProvider timeProvider) : ICommunityService
{
    private const int RequiredVisitsForVerification = 5;
    private const int RequiredReviewsForVerification = 3;
    private const int MaxAllowedRemovedReviewsForVerification = 0;
    private const int RequiredAccountAgeDaysForVerification = 30;

    /**
     * Consulta ranking público de exploradores ordenados pela pontuação combinada de contribuição.
     */
    public async Task<Result<PagedResponseDto<ExplorerListItemDto>>> ListExplorersAsync(
        PaginationQueryDto pagination,
        string? neighborhood = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var usersQuery = context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(neighborhood))
        {
            var pattern = $"%{neighborhood.Trim()}%";
            usersQuery = usersQuery.Where(u => u.Neighborhood != null && EF.Functions.ILike(u.Neighborhood, pattern));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            usersQuery = usersQuery.Where(u => EF.Functions.ILike(u.Name, pattern));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var usersData = await usersQuery
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.AvatarUrl,
                u.Neighborhood,
                u.CreatedAt,
                ReviewsCount = u.Reviews.Count(r => r.RemovedAt == null),
                RemovedReviewsCount = u.Reviews.Count(r => r.RemovedAt != null),
                ApprovedPlacesCount = context.Places.Count(p => p.AuthorId == u.Id && p.Status == PlaceStatus.Approved),
                VisitsCount = u.Visits.Count(),
                AchievementsCount = u.UserAchievements.Count()
            })
            .ToListAsync(cancellationToken);

        var rankedList = usersData
            .Select(u =>
            {
                var score = (u.ReviewsCount * 2) + (u.ApprovedPlacesCount * 5) + u.VisitsCount;
                var accountAgeDays = Math.Max(0, (int)(now - u.CreatedAt).TotalDays);

                var isVerified = u.VisitsCount >= RequiredVisitsForVerification
                    && u.ReviewsCount >= RequiredReviewsForVerification
                    && u.RemovedReviewsCount <= MaxAllowedRemovedReviewsForVerification
                    && accountAgeDays >= RequiredAccountAgeDaysForVerification;

                var badge = CalculateTitleBadge(score, u.AchievementsCount);

                return new ExplorerListItemDto(
                    u.Id,
                    u.Name,
                    u.AvatarUrl,
                    u.Neighborhood,
                    score,
                    u.ReviewsCount,
                    u.ApprovedPlacesCount,
                    u.VisitsCount,
                    u.AchievementsCount,
                    badge,
                    isVerified,
                    u.CreatedAt
                );
            })
            .OrderByDescending(x => x.ContributionScore)
            .ThenByDescending(x => x.AchievementsCount)
            .ToList();

        var total = rankedList.Count;
        var page = pagination.Page;
        var pageSize = pagination.PageSize;
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var pagedItems = rankedList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResponseDto<ExplorerListItemDto>(
            pagedItems,
            page,
            pageSize,
            total,
            totalPages
        );
    }

    /**
     * Consulta dados públicos do perfil do usuário com histórico de contribuições públicas.
     */
    public async Task<Result<CommunityUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var visitsCount = await context.Visits.CountAsync(v => v.UserId == userId, cancellationToken);
        var publishedReviewsCount = await context.Reviews.CountAsync(r => r.UserId == userId && r.RemovedAt == null, cancellationToken);
        var removedReviewsCount = await context.Reviews.CountAsync(r => r.UserId == userId && r.RemovedAt != null, cancellationToken);
        var approvedPlacesCount = await context.Places.CountAsync(p => p.AuthorId == userId && p.Status == PlaceStatus.Approved, cancellationToken);
        var achievementsCount = await context.UserAchievements.CountAsync(ua => ua.UserId == userId, cancellationToken);

        var score = (publishedReviewsCount * 2) + (approvedPlacesCount * 5) + visitsCount;
        var accountAgeDays = Math.Max(0, (int)(now - user.CreatedAt).TotalDays);

        var isVerified = visitsCount >= RequiredVisitsForVerification
            && publishedReviewsCount >= RequiredReviewsForVerification
            && removedReviewsCount <= MaxAllowedRemovedReviewsForVerification
            && accountAgeDays >= RequiredAccountAgeDaysForVerification;

        var titleBadge = CalculateTitleBadge(score, achievementsCount);

        var recentReviews = await context.Reviews
            .AsNoTracking()
            .Include(r => r.Place)
            .Include(r => r.User)
            .Include(r => r.UsefulVotes)
            .Include(r => r.Comments)
            .Where(r => r.UserId == userId && r.RemovedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(ReviewMappings.ToResponseProjection(userId))
            .ToListAsync(cancellationToken);

        var approvedPlaces = await context.Places
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Include(p => p.Photos)
            .Include(p => p.PlaceTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.AuthorId == userId && p.Status == PlaceStatus.Approved)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(PlaceMappings.ToSummaryProjection)
            .ToListAsync(cancellationToken);

        var unlockedAchievements = await context.UserAchievements
            .AsNoTracking()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.UnlockedAt)
            .Select(ua => new UserAchievementProgressDto(
                ua.Achievement.Id,
                ua.Achievement.Name,
                ua.Achievement.Description,
                ua.Achievement.Icon,
                ua.Achievement.Color,
                ua.Achievement.RequiredQuantity,
                ua.Achievement.RequiredQuantity,
                true,
                ua.UnlockedAt
            ))
            .ToListAsync(cancellationToken);

        return new CommunityUserProfileDto(
            user.Id,
            user.Name,
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            score,
            visitsCount,
            publishedReviewsCount,
            approvedPlacesCount,
            achievementsCount,
            titleBadge,
            isVerified,
            user.CreatedAt,
            recentReviews,
            approvedPlaces,
            unlockedAchievements
        );
    }

    private static string CalculateTitleBadge(int score, int achievementsCount)
    {
        if (score >= 100 || achievementsCount >= 10)
            return "Lenda Local";
        if (score >= 50 || achievementsCount >= 6)
            return "Guia Experiente";
        if (score >= 20 || achievementsCount >= 3)
            return "Explorador Ativo";
        return "Caminhante Curioso";
    }
}
