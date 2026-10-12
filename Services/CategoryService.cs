using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pela gestão do catálogo de categorias de lugares.
 * Controla unicidade de nomenclatura e proteção contra exclusão com dependências órfãs.
 */
public sealed class CategoryService(AppDbContext context) : ICategoryService
{
    /**
     * Lista todas as categorias de forma paginada e ordenada por nome.
     */
    public async Task<Result<PagedResponseDto<CategoryResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default)
    {
        return await context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(CategoryMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Obtém uma categoria específica pelo seu identificador único.
     */
    public async Task<Result<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(existingCategory => existingCategory.Id == id)
            .Select(CategoryMappings.ToResponseProjection)
            .SingleOrDefaultAsync(cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        return category;
    }

    /**
     * Cria uma nova categoria no sistema após verificar unicidade do nome.
     */
    public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();

        /** Evita duplicação de categorias ignorando maiúsculas e minúsculas. */
        var nameExists = await context.Categories
            .AnyAsync(category => category.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

        if (nameExists)
            return CategoryErrors.NameTaken;

        var category = new Category
        {
            Name = trimmedName
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return category.ToResponse();
    }

    /**
     * Atualiza o nome de uma categoria existente.
     */
    public async Task<Result<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .SingleOrDefaultAsync(existingCategory => existingCategory.Id == id, cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        /** Se houver novo nome, confere se não colide com outra categoria já cadastrada. */
        if (request.Name is not null)
        {
            var trimmedName = request.Name.Trim();
            var nameExists = await context.Categories
                .AnyAsync(existingCategory => existingCategory.Id != id && existingCategory.Name.ToLower() == trimmedName.ToLower(), cancellationToken);

            if (nameExists)
                return CategoryErrors.NameTaken;
        }

        category.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return category.ToResponse();
    }

    /**
     * Remove uma categoria verificando integridade referencial antes da deleção.
     * Impede a remoção se houver lugares ativos vinculados (ON DELETE RESTRICT).
     */
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .SingleOrDefaultAsync(existingCategory => existingCategory.Id == id, cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        /** Verifica presença de lugares vinculados para retornar conflito legível em vez de erro 500 do banco. */
        var hasPlaces = await context.Places
            .AnyAsync(place => place.CategoryId == id, cancellationToken);

        if (hasPlaces)
            return CategoryErrors.InUse;

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
