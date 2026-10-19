using soromaps_api.DTOs.Tags;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface ITagService
{
    Task<Result<IReadOnlyList<TagResponseDto>>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TagResponseDto>> CreateAsync(CreateTagDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
