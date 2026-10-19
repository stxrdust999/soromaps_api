using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Tags;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class TagService(AppDbContext context) : ITagService
{
    public async Task<Result<IReadOnlyList<TagResponseDto>>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await context.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .Select(TagMappings.ToResponseProjection)
            .ToListAsync(cancellationToken);

        return tags;
    }

    public async Task<Result<TagResponseDto>> CreateAsync(CreateTagDto request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(trimmedName)
            : SlugHelper.GenerateSlug(request.Slug);

        var exists = await context.Tags
            .AnyAsync(tag => tag.Name.ToLower() == trimmedName.ToLower() || tag.Slug == slug, cancellationToken);

        if (exists)
            return TagErrors.NameTaken;

        var newTag = new Tag
        {
            Name = trimmedName,
            Slug = slug
        };

        try
        {
            context.Tags.Add(newTag);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return TagErrors.NameTaken;
        }

        return newTag.ToResponse();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await context.Tags
            .SingleOrDefaultAsync(existingTag => existingTag.Id == id, cancellationToken);

        if (tag is null)
            return TagErrors.NotFound;

        context.Tags.Remove(tag);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
