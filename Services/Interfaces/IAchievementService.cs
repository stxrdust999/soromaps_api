using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface IAchievementService
{
    Task<Result<PagedResponseDto<AchievementResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<Result<AchievementResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<AchievementResponseDto>> CreateAsync(CreateAchievementDto request, CancellationToken cancellationToken = default);
    Task<Result<AchievementResponseDto>> UpdateAsync(Guid id, UpdateAchievementDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
