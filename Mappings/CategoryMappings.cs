using System.Linq.Expressions;
using soromaps_api.DTOs.Categories;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre a entidade Category e seus DTOs.
 * Fornece expressões de projeção para o EF Core traduzir em SQL e métodos de extensão para objetos em memória.
 */
public static class CategoryMappings
{
    /**
     * Expressão de projeção otimizada para listagem e consulta de categorias.
     * Gera uma query SQL trazendo estritamente as colunas do DTO, ignorando coleções de navegação (como Places)
     * e evitando o carregamento desnecessário do grafo de dependências pelo ChangeTracker.
     */
    public static readonly Expression<Func<Category, CategoryResponseDto>> ToResponseProjection = category =>
        new CategoryResponseDto(
            category.Id,
            category.Name,
            category.CreatedAt,
            category.UpdatedAt
        );

    /**
     * Converte uma entidade Category carregada na memória para o DTO de resposta.
     * Usado após operações de criação ou edição direta via CategoryService.
     */
    public static CategoryResponseDto ToResponse(this Category category) =>
        new(
            category.Id,
            category.Name,
            category.CreatedAt,
            category.UpdatedAt
        );

    /**
     * Aplica alterações parciais vindas de UpdateCategoryDto sobre a entidade rastreada,
     * atualizando apenas os campos preenchidos e sanitizando espaços em branco.
     */
    public static void ApplyUpdate(this Category category, UpdateCategoryDto request)
    {
        if (request.Name is not null)
            category.Name = request.Name.Trim();
    }
}
