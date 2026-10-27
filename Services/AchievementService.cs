using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pelo catálogo de conquistas do sistema.
 * Gerencia a consulta pública e o cadastro/manutenção por administradores.
 */
public sealed class AchievementService(
    AppDbContext context,
    ICurrentUserService currentUserService,
    IAchievementEvaluatorService evaluatorService) : IAchievementService
{
    /**
     * Lista todas as conquistas cadastradas de forma paginada e ordenada por nome.
     */
    public async Task<Result<PagedResponseDto<AchievementResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default)
    {
        var totalExplorersCount = await context.Users.AsNoTracking().CountAsync(cancellationToken);

        return await context.Achievements
            .AsNoTracking()
            .OrderBy(achievement => achievement.Name)
            .Select(AchievementMappings.ToResponseProjection(totalExplorersCount))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Obtém uma conquista específica pelo seu identificador único.
     */
    public async Task<Result<AchievementResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var totalExplorersCount = await context.Users.AsNoTracking().CountAsync(cancellationToken);

        var achievement = await context.Achievements
            .AsNoTracking()
            .Where(existingAchievement => existingAchievement.Id == id)
            .Select(AchievementMappings.ToResponseProjection(totalExplorersCount))
            .SingleOrDefaultAsync(cancellationToken);

        if (achievement is null)
            return AchievementErrors.NotFound;

        return achievement;
    }

    /**
     * Cadastra uma nova conquista no sistema após validar a unicidade do título.
     */
    public async Task<Result<AchievementResponseDto>> CreateAsync(CreateAchievementDto request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();

        var nameExists = await context.Achievements
            .AnyAsync(existingAchievement => existingAchievement.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (nameExists)
            return AchievementErrors.NameTaken;

        var achievement = new Achievement
        {
            Name = trimmedName,
            Description = request.Description.Trim(),
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "award" : request.Icon.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#F59E0B" : request.Color.Trim(),
            EventType = request.EventType,
            RequiredQuantity = request.RequiredQuantity,
            TargetType = request.TargetType,
            TargetValue = string.IsNullOrWhiteSpace(request.TargetValue) ? null : request.TargetValue.Trim(),
            IsActive = request.IsActive
        };

        context.Achievements.Add(achievement);
        await context.SaveChangesAsync(cancellationToken);

        return achievement.ToResponse();
    }

    /**
     * Atualiza dados de uma conquista existente.
     */
    public async Task<Result<AchievementResponseDto>> UpdateAsync(Guid id, UpdateAchievementDto request, CancellationToken cancellationToken = default)
    {
        var achievement = await context.Achievements
            .SingleOrDefaultAsync(existingAchievement => existingAchievement.Id == id, cancellationToken);

        if (achievement is null)
            return AchievementErrors.NotFound;

        if (request.Name is not null)
        {
            var trimmedName = request.Name.Trim();
            var nameExists = await context.Achievements
                .AnyAsync(existingAchievement => existingAchievement.Id != id && existingAchievement.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

            if (nameExists)
                return AchievementErrors.NameTaken;
        }

        achievement.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        var totalExplorersCount = await context.Users.AsNoTracking().CountAsync(cancellationToken);
        var holdersCount = await context.UserAchievements.AsNoTracking().CountAsync(ua => ua.AchievementId == id, cancellationToken);
        var pct = totalExplorersCount > 0 ? Math.Round((double)holdersCount / totalExplorersCount * 100.0, 1) : 0.0;

        return achievement.ToResponse(holdersCount, pct);
    }

    /**
     * Remove uma conquista do sistema.
     */
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var achievement = await context.Achievements
            .SingleOrDefaultAsync(existingAchievement => existingAchievement.Id == id, cancellationToken);

        if (achievement is null)
            return AchievementErrors.NotFound;

        context.Achievements.Remove(achievement);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /**
     * Lista o progresso de todas as conquistas ativas para o explorador autenticado, fornecendo a regra objetiva de desbloqueio.
     */
    public async Task<Result<IReadOnlyList<UserAchievementProgressDto>>> ListMyAchievementsAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId;

        var achievements = await context.Achievements
            .AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var unlockedDict = await context.UserAchievements
            .AsNoTracking()
            .Where(ua => ua.UserId == userId)
            .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.UnlockedAt, cancellationToken);

        var result = new List<UserAchievementProgressDto>();

        foreach (var achievement in achievements)
        {
            var isUnlocked = unlockedDict.TryGetValue(achievement.Id, out var unlockedAt);
            var currentProgress = isUnlocked
                ? achievement.RequiredQuantity
                : await evaluatorService.CalculateProgressAsync(userId, achievement, cancellationToken);

            var descriptionText = isUnlocked
                ? achievement.Description
                : BuildCriteriaText(achievement);

            result.Add(new UserAchievementProgressDto(
                achievement.Id,
                achievement.Name,
                descriptionText,
                achievement.Icon,
                achievement.Color,
                currentProgress,
                achievement.RequiredQuantity,
                isUnlocked,
                isUnlocked ? unlockedAt : null
            ));
        }

        return result;
    }

    /**
     * Constrói o texto objetivo do critério de desbloqueio de uma conquista com base no evento, meta e alvo configurados.
     */
    private static string BuildCriteriaText(Achievement achievement)
    {
        return achievement.EventType switch
        {
            AchievementEventType.Visit => achievement.TargetType switch
            {
                AchievementTargetType.Category => $"Registrar {achievement.RequiredQuantity} visita(s) em locais desta categoria",
                AchievementTargetType.Neighborhood => $"Registrar {achievement.RequiredQuantity} visita(s) no bairro {achievement.TargetValue}",
                AchievementTargetType.DistinctCategories => $"Visitar locais de {achievement.RequiredQuantity} categoria(s) diferente(s)",
                AchievementTargetType.DistinctNeighborhoods => $"Visitar locais em {achievement.RequiredQuantity} bairro(s) diferente(s)",
                _ => $"Registrar {achievement.RequiredQuantity} visita(s) em locais de Sorocaba"
            },
            AchievementEventType.Review => achievement.TargetType switch
            {
                AchievementTargetType.Category => $"Publicar {achievement.RequiredQuantity} avaliação(ões) em locais desta categoria",
                AchievementTargetType.Neighborhood => $"Publicar {achievement.RequiredQuantity} avaliação(ões) no bairro {achievement.TargetValue}",
                AchievementTargetType.DistinctCategories => $"Avaliar locais de {achievement.RequiredQuantity} categoria(s) diferente(s)",
                AchievementTargetType.DistinctNeighborhoods => $"Avaliar locais em {achievement.RequiredQuantity} bairro(s) diferente(s)",
                _ => $"Publicar {achievement.RequiredQuantity} avaliação(ões) em locais da cidade"
            },
            AchievementEventType.CreatePlace => $"Cadastrar {achievement.RequiredQuantity} ponto(s) aprovado(s) no mapa",
            AchievementEventType.Favorite => $"Salvar {achievement.RequiredQuantity} local(is) nos seus favoritos",
            AchievementEventType.Comment => $"Publicar {achievement.RequiredQuantity} comentário(s) útil(eis) na comunidade",
            _ => achievement.Description
        };
    }

    public async Task<Result<AchievementEstimateResponseDto>> EstimateAsync(AchievementEstimateQueryDto query, CancellationToken cancellationToken = default)
    {
        var totalExplorers = await context.Users.AsNoTracking().CountAsync(cancellationToken);

        var dummyAchievement = new Achievement
        {
            EventType = query.EventType,
            TargetType = query.TargetType,
            TargetValue = query.TargetValue,
            RequiredQuantity = query.RequiredQuantity
        };

        List<Guid> candidateUserIds = query.EventType switch
        {
            AchievementEventType.Visit => await context.Visits.AsNoTracking().Select(v => v.UserId).Distinct().ToListAsync(cancellationToken),
            AchievementEventType.Review => await context.Reviews.AsNoTracking().Where(r => r.RemovedAt == null && r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToListAsync(cancellationToken),
            AchievementEventType.Favorite => await context.Favorites.AsNoTracking().Select(f => f.UserId).Distinct().ToListAsync(cancellationToken),
            AchievementEventType.CreatePlace => await context.Places.AsNoTracking().Where(p => p.Status == PlaceStatus.Approved && p.AuthorId.HasValue).Select(p => p.AuthorId!.Value).Distinct().ToListAsync(cancellationToken),
            AchievementEventType.Comment => await context.Comments.AsNoTracking().Where(c => c.RemovedAt == null && c.UserId.HasValue).Select(c => c.UserId!.Value).Distinct().ToListAsync(cancellationToken),
            _ => []
        };

        var eligibleCount = 0;
        foreach (var candidateUserId in candidateUserIds)
        {
            var count = await evaluatorService.CalculateProgressAsync(candidateUserId, dummyAchievement, cancellationToken);
            if (count >= query.RequiredQuantity)
                eligibleCount++;
        }

        var percentage = totalExplorers > 0
            ? Math.Round((double)eligibleCount / totalExplorers * 100.0, 1)
            : 0.0;

        return new AchievementEstimateResponseDto(
            totalExplorers,
            eligibleCount,
            percentage
        );
    }
}
