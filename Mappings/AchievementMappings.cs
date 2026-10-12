using System.Linq.Expressions;
using soromaps_api.DTOs.Achievements;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre a entidade Achievement e seus DTOs.
 * Fornece expressões de projeção para o EF Core traduzir em SQL e métodos de extensão para objetos em memória.
 */
public static class AchievementMappings
{
    /**
     * Expressão de projeção otimizada para listagem de conquistas.
     * Gera uma query SQL direta no banco trazendo apenas as colunas exibidas na listagem,
     * permitindo que o PostgreSQL use os índices e otimize a paginação sem reter entidades no ChangeTracker.
     */
    public static readonly Expression<Func<Achievement, AchievementResponseDto>> ToResponseProjection = achievement =>
        new AchievementResponseDto(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.CreatedAt,
            achievement.UpdatedAt
        );

    /**
     * Converte uma entidade Achievement carregada na memória para o DTO de resposta.
     * Usado após operações de criação ou edição direta via AchievementService.
     */
    public static AchievementResponseDto ToResponse(this Achievement achievement) =>
        new(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.CreatedAt,
            achievement.UpdatedAt
        );

    /**
     * Aplica alterações parciais vindas de UpdateAchievementDto sobre a entidade rastreada,
     * ignorando propriedades nulas e sanitizando espaços em branco nos textos.
     */
    public static void ApplyUpdate(this Achievement achievement, UpdateAchievementDto request)
    {
        if (request.Name is not null)
            achievement.Name = request.Name.Trim();

        if (request.Description is not null)
            achievement.Description = request.Description.Trim();
    }
}
