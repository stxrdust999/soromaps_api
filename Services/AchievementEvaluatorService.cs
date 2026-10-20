using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.Models;
using soromaps_api.Models.Enums;

namespace soromaps_api.Services;

/**
 * Contrato para o avaliador idempotente de regras de gamificação e conquistas.
 */
public interface IAchievementEvaluatorService
{
    /**
     * Avalia eventos de atividade (visita, avaliação, favorito, local cadastrado, comentário) e desbloqueia novas conquistas para o usuário.
     */
    Task EvaluateForUserAsync(Guid userId, AchievementEventType eventType, CancellationToken cancellationToken = default);

    /**
     * Calcula o progresso numérico atual do usuário em relação à meta de uma conquista específica.
     */
    Task<int> CalculateProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken = default);
}

/**
 * Avaliador idempotente de conquistas do explorador.
 * Calcula o progresso com base no tipo de evento, escopo de categorias/bairros e metas quantitativas.
 */
public sealed class AchievementEvaluatorService(AppDbContext context, TimeProvider timeProvider) : IAchievementEvaluatorService
{
    /**
     * Processa conquistas pendentes elegíveis para o evento e persiste novos desbloqueios de forma idempotente.
     */
    public async Task EvaluateForUserAsync(Guid userId, AchievementEventType eventType, CancellationToken cancellationToken = default)
    {
        var activeAchievements = await context.Achievements
            .AsNoTracking()
            .Where(achievement => achievement.IsActive && achievement.EventType == eventType)
            .ToListAsync(cancellationToken);

        if (activeAchievements.Count == 0)
            return;

        var alreadyUnlockedIds = await context.UserAchievements
            .AsNoTracking()
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync(cancellationToken);

        var pendingAchievements = activeAchievements
            .Where(a => !alreadyUnlockedIds.Contains(a.Id))
            .ToList();

        if (pendingAchievements.Count == 0)
            return;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var newlyUnlocked = new List<UserAchievement>();

        foreach (var achievement in pendingAchievements)
        {
            var count = await CalculateProgressAsync(userId, achievement, cancellationToken);
            if (count >= achievement.RequiredQuantity)
            {
                newlyUnlocked.Add(new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    UnlockedAt = now
                });
            }
        }

        if (newlyUnlocked.Count > 0)
        {
            context.UserAchievements.AddRange(newlyUnlocked);
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                /** Trata concorrência silenciosamente via restrição de chave primária composta (user_id, achievement_id). */
            }
        }
    }

    /**
     * Roteia o cálculo do progresso do usuário de acordo com a natureza do evento da conquista.
     */
    public async Task<int> CalculateProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        return achievement.EventType switch
        {
            AchievementEventType.Visit => await CalculateVisitProgressAsync(userId, achievement, cancellationToken),
            AchievementEventType.Review => await CalculateReviewProgressAsync(userId, achievement, cancellationToken),
            AchievementEventType.Favorite => await CalculateFavoriteProgressAsync(userId, achievement, cancellationToken),
            AchievementEventType.CreatePlace => await CalculateCreatePlaceProgressAsync(userId, achievement, cancellationToken),
            AchievementEventType.Comment => await context.Comments.CountAsync(c => c.UserId == userId && c.RemovedAt == null, cancellationToken),
            _ => 0
        };
    }

    private async Task<int> CalculateVisitProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        var query = context.Visits
            .AsNoTracking()
            .Where(v => v.UserId == userId);

        return achievement.TargetType switch
        {
            AchievementTargetType.Category => await query.CountAsync(v => v.Place.Category.Slug == achievement.TargetValue, cancellationToken),
            AchievementTargetType.Neighborhood => await query.CountAsync(v => v.Place.Neighborhood == achievement.TargetValue, cancellationToken),
            AchievementTargetType.DistinctCategories => await query.Select(v => v.Place.CategoryId).Distinct().CountAsync(cancellationToken),
            AchievementTargetType.DistinctNeighborhoods => await query.Select(v => v.Place.Neighborhood).Distinct().CountAsync(cancellationToken),
            _ => await query.CountAsync(cancellationToken)
        };
    }

    private async Task<int> CalculateReviewProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        var query = context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RemovedAt == null);

        return achievement.TargetType switch
        {
            AchievementTargetType.Category => await query.CountAsync(r => r.Place.Category.Slug == achievement.TargetValue, cancellationToken),
            AchievementTargetType.Neighborhood => await query.CountAsync(r => r.Place.Neighborhood == achievement.TargetValue, cancellationToken),
            AchievementTargetType.DistinctCategories => await query.Select(r => r.Place.CategoryId).Distinct().CountAsync(cancellationToken),
            AchievementTargetType.DistinctNeighborhoods => await query.Select(r => r.Place.Neighborhood).Distinct().CountAsync(cancellationToken),
            _ => await query.CountAsync(cancellationToken)
        };
    }

    private async Task<int> CalculateFavoriteProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        var query = context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId);

        return achievement.TargetType switch
        {
            AchievementTargetType.Category => await query.CountAsync(f => f.Place.Category.Slug == achievement.TargetValue, cancellationToken),
            AchievementTargetType.Neighborhood => await query.CountAsync(f => f.Place.Neighborhood == achievement.TargetValue, cancellationToken),
            _ => await query.CountAsync(cancellationToken)
        };
    }

    private async Task<int> CalculateCreatePlaceProgressAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        var query = context.Places
            .AsNoTracking()
            .Where(p => p.AuthorId == userId && p.Status == PlaceStatus.Approved);

        return achievement.TargetType switch
        {
            AchievementTargetType.Category => await query.CountAsync(p => p.Category.Slug == achievement.TargetValue, cancellationToken),
            AchievementTargetType.Neighborhood => await query.CountAsync(p => p.Neighborhood == achievement.TargetValue, cancellationToken),
            _ => await query.CountAsync(cancellationToken)
        };
    }
}
