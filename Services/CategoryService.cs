using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class CategoryService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : ICategoryService
{
    public async Task<Result<PagedResponseDto<CategoryResponseDto>>> ListAsync(
        PaginationQueryDto pagination,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = context.Categories.AsNoTracking();

        var canSeeInactive = includeInactive && currentUser.IsInRole(UserRole.Admin);
        if (!canSeeInactive)
            query = query.Where(category => category.IsActive);

        var oneWeekAgoUtc = timeProvider.GetUtcNow().UtcDateTime.AddDays(-7);

        return await query
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(CategoryMappings.ToResponseProjection(oneWeekAgoUtc))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    public async Task<Result<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var oneWeekAgoUtc = timeProvider.GetUtcNow().UtcDateTime.AddDays(-7);

        var category = await context.Categories
            .AsNoTracking()
            .Where(existingCategory => existingCategory.Id == id)
            .Select(CategoryMappings.ToResponseProjection(oneWeekAgoUtc))
            .SingleOrDefaultAsync(cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        return category;
    }

    public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var resolvedSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(trimmedName)
            : SlugHelper.GenerateSlug(request.Slug);

        var duplicateExists = await context.Categories
            .AnyAsync(category =>
                category.Name.ToLower() == trimmedName.ToLower() ||
                category.Slug == resolvedSlug,
                cancellationToken);

        if (duplicateExists)
            return CategoryErrors.NameTaken;

        var nextOrder = request.SortOrder ?? (short)(await context.Categories
            .Select(category => (int?)category.SortOrder)
            .MaxAsync(cancellationToken) + 1 ?? 1);

        var category = new Category
        {
            Name = trimmedName,
            Slug = resolvedSlug,
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "map-pin" : request.Icon.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#2563EB" : request.Color.Trim().ToUpperInvariant(),
            SortOrder = nextOrder,
            IsActive = request.IsActive
        };

        try
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return CategoryErrors.NameTaken;
        }

        return category.ToResponse();
    }

    public async Task<Result<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .SingleOrDefaultAsync(existingCategory => existingCategory.Id == id, cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        var candidateName = request.Name?.Trim() ?? category.Name;
        var candidateSlug = request.Slug is not null
            ? SlugHelper.GenerateSlug(request.Slug)
            : request.Name is not null
                ? SlugHelper.GenerateSlug(candidateName)
                : category.Slug;

        var duplicateExists = await context.Categories
            .AnyAsync(existingCategory =>
                existingCategory.Id != id &&
                (existingCategory.Name.ToLower() == candidateName.ToLower() || existingCategory.Slug == candidateSlug),
                cancellationToken);

        if (duplicateExists)
            return CategoryErrors.NameTaken;

        category.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> ReorderAsync(ReorderCategoriesDto request, CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories
            .Where(category => request.CategoryIds.Contains(category.Id))
            .ToListAsync(cancellationToken);

        for (var index = 0; index < request.CategoryIds.Count; index++)
        {
            var targetId = request.CategoryIds[index];
            var matched = categories.FirstOrDefault(category => category.Id == targetId);
            if (matched is not null)
                matched.SortOrder = (short)(index + 1);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, Guid? reassignToCategoryId = null, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .SingleOrDefaultAsync(existingCategory => existingCategory.Id == id, cancellationToken);

        if (category is null)
            return CategoryErrors.NotFound;

        var hasPlaces = await context.Places
            .AnyAsync(place => place.CategoryId == id, cancellationToken);

        if (hasPlaces)
        {
            if (!reassignToCategoryId.HasValue)
                return CategoryErrors.InUse;

            if (reassignToCategoryId.Value == id)
                return CategoryErrors.InvalidReassignTarget;

            var targetExists = await context.Categories
                .AnyAsync(existingCategory => existingCategory.Id == reassignToCategoryId.Value, cancellationToken);

            if (!targetExists)
                return CategoryErrors.InvalidReassignTarget;

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            await context.Places
                .Where(place => place.CategoryId == id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(place => place.CategoryId, reassignToCategoryId.Value),
                    cancellationToken);

            context.Categories.Remove(category);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result.Success();
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
