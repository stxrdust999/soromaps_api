using Microsoft.EntityFrameworkCore;
using soromaps_api.DTOs.Common;

namespace soromaps_api.Extensions;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResponseDto<TItem>> ToPagedResponseAsync<TItem>(
        this IQueryable<TItem> query,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponseDto<TItem>(
            items,
            pagination.Page,
            pagination.PageSize,
            totalCount,
            totalPages
        );
    }
}
