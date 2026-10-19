using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<PagedResponseDto<CategoryResponseDto>>> ListAsync(PaginationQueryDto pagination, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task<Result> ReorderAsync(ReorderCategoriesDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, Guid? reassignToCategoryId = null, CancellationToken cancellationToken = default);
}
