using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pelo catálogo de conquistas do sistema.
 * Gerencia a consulta pública e o cadastro/manutenção por administradores.
 */
public sealed class AchievementService(AppDbContext context) : IAchievementService
{
    /**
     * Lista todas as conquistas cadastradas de forma paginada e ordenada por nome.
     */
    public async Task<Result<PagedResponseDto<AchievementResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default)
    {
        return await context.Achievements
            .AsNoTracking()
            .OrderBy(achievement => achievement.Name)
            .Select(AchievementMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Obtém uma conquista específica pelo seu identificador único.
     */
    public async Task<Result<AchievementResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var achievement = await context.Achievements
            .AsNoTracking()
            .Where(existingAchievement => existingAchievement.Id == id)
            .Select(AchievementMappings.ToResponseProjection)
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

        /** Evita colisão de nomes ignorando maiúsculas e minúsculas. */
        var nameExists = await context.Achievements
            .AnyAsync(existingAchievement => existingAchievement.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (nameExists)
            return AchievementErrors.NameTaken;

        var achievement = new Achievement
        {
            Name = trimmedName,
            Description = request.Description.Trim()
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

        /** Se houver novo nome, confere se não entra em conflito com outra conquista existente. */
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

        return achievement.ToResponse();
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
}
