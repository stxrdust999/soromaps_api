namespace soromaps_api.DTOs.Common;

public sealed record PagedResponseDto<TItem>(
    IReadOnlyCollection<TItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
