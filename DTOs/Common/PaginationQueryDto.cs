using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Common;

public sealed record PaginationQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "common.invalid_page")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "common.invalid_page_size")]
    public int PageSize { get; init; } = 20;
}
