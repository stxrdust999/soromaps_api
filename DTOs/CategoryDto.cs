using soromaps_api.Models;

namespace soromaps_api.DTOs
{
    public class CategoryDto
    {
        public record CategoryResponseDto(
        Guid CategoryId,
        string CategoryDesc
    )
        {
            public static CategoryResponseDto FromEntity(Category category) =>
                new(
                    category.CategoryId,
                    category.CategoryDesc
                );
        }

        public record CreateCategoryDto(string CategoryDesc);

        public record UpdateCategoryDto(string? CategoryDesc);
    }
}
