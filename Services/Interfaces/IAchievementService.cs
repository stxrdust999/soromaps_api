using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para catálogo, manutenção e progresso de conquistas de gamificação.
 */
public interface IAchievementService
{
    /**
     * Lista todas as conquistas cadastradas no sistema de forma paginada.
     */
    Task<Result<PagedResponseDto<AchievementResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);

    /**
     * Obtém os detalhes de uma conquista pelo seu identificador único.
     */
    Task<Result<AchievementResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /**
     * Cadastra uma nova conquista no catálogo administrativo com suas regras de gatilho.
     */
    Task<Result<AchievementResponseDto>> CreateAsync(CreateAchievementDto request, CancellationToken cancellationToken = default);

    /**
     * Atualiza as propriedades, regras ou status de ativação de uma conquista existente.
     */
    Task<Result<AchievementResponseDto>> UpdateAsync(Guid id, UpdateAchievementDto request, CancellationToken cancellationToken = default);

    /**
     * Remove uma conquista e seu histórico de desbloqueios do sistema.
     */
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /**
     * Lista as conquistas ativas acompanhadas do percentual e status de desbloqueio do usuário logado.
     */
    Task<Result<IReadOnlyList<UserAchievementProgressDto>>> ListMyAchievementsAsync(CancellationToken cancellationToken = default);

    /**
     * Simula a quantidade e percentual de usuários que atingiriam os critérios de uma regra antes de cadastrá-la.
     */
    Task<Result<AchievementEstimateResponseDto>> EstimateAsync(AchievementEstimateQueryDto query, CancellationToken cancellationToken = default);
}
