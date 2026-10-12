using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por regras de negócio e persistência de locais/pontos no mapa.
 * Garante filtros de moderação, checagem de autor (IDOR) e paginação eficiente.
 */
public sealed class PlaceService(AppDbContext context, ICurrentUserService currentUser) : IPlaceService
{
    /**
     * Retorna a listagem paginada de lugares públicos com status 'Approved'.
     * Utiliza projeção estática para carregar apenas os campos necessários na view do mapa.
     */
    public async Task<Result<PagedResponseDto<PlaceSummaryDto>>> ListApprovedAsync(
        PaginationQueryDto pagination,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        /** Filtra inicialmente apenas lugares aprovados para o mapa público. */
        var query = context.Places
            .AsNoTracking()
            .Where(place => place.Status == PlaceStatus.Approved);

        /** Aplica filtro opcional por categoria sem reexecutar query antecipadamente. */
        if (categoryId.HasValue)
            query = query.Where(place => place.CategoryId == categoryId.Value);

        /** Ordena pelos mais recentes e executa contagem e slice paginado via extension. */
        return await query
            .OrderByDescending(place => place.CreatedAt)
            .Select(PlaceMappings.ToSummaryProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Busca os detalhes completos de um lugar pelo seu identificador.
     * Lugares não aprovados só podem ser visualizados pelo seu próprio autor ou por administradores.
     */
    public async Task<Result<PlaceResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        /** Carrega o lugar e inclui os dados básicos do autor para retorno. */
        var place = await context.Places
            .AsNoTracking()
            .Include(existingPlace => existingPlace.Author)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        /** Verifica privilégios para exibição de lugares em moderação/pendentes. */
        var isAuthor = currentUser.IsAuthenticated && place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (place.Status != PlaceStatus.Approved && !isAuthor && !isAdmin)
            return PlaceErrors.NotFound;

        return place.ToResponse();
    }

    /**
     * Cadastra uma nova sugestão de local vinculado ao usuário autenticado.
     * O local nasce com status 'Pending' até aprovação pela equipe de moderação/administração.
     */
    public async Task<Result<PlaceResponseDto>> CreateAsync(CreatePlaceDto request, CancellationToken cancellationToken = default)
    {
        /** Valida se a categoria indicada existe antes de vincular a chave estrangeira. */
        var categoryExists = await context.Categories
            .AnyAsync(category => category.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            return PlaceErrors.CategoryNotFound;

        /** Instancia a entidade associando o autor a partir dos claims do JWT. */
        var place = new Place
        {
            CategoryId = request.CategoryId,
            AuthorId = currentUser.UserId,
            Name = request.Name.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Neighborhood = request.Neighborhood.Trim(),
            About = request.About.Trim(),
            Description = request.Description.Trim(),
            HasWifi = request.HasWifi,
            IsPetFriendly = request.IsPetFriendly,
            Status = PlaceStatus.Pending
        };

        context.Places.Add(place);
        await context.SaveChangesAsync(cancellationToken);

        /** Carrega a navegação do autor recém-associado para montar o DTO de resposta. */
        await context.Entry(place).Reference(p => p.Author).LoadAsync(cancellationToken);

        return place.ToResponse();
    }

    /**
     * Atualiza dados cadastrais de um lugar existente.
     * Impede mutação por outros usuários e mantém o status aprovado inalterado.
     */
    public async Task<Result<PlaceResponseDto>> UpdateAsync(Guid id, UpdatePlaceDto request, CancellationToken cancellationToken = default)
    {
        /** Carrega o lugar do banco rastreado para atualização. */
        var place = await context.Places
            .Include(existingPlace => existingPlace.Author)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        /** Garante que apenas o autor original ou um administrador possam editar. */
        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        /** Se houver troca de categoria, valida a existência da nova categoria. */
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await context.Categories
                .AnyAsync(category => category.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
                return PlaceErrors.CategoryNotFound;
        }

        /** Aplica as alterações parciais mantendo o status de aprovação intacto. */
        place.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return place.ToResponse();
    }

    /**
     * Atualiza o status de aprovação de um local na fila de moderação.
     * Ação restrita a administradores.
     */
    public async Task<Result<PlaceResponseDto>> UpdateStatusAsync(Guid id, UpdatePlaceStatusDto request, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .Include(existingPlace => existingPlace.Author)
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        /** Define o novo status deliberado pelo administrador. */
        place.Status = request.Status;
        await context.SaveChangesAsync(cancellationToken);

        return place.ToResponse();
    }

    /**
     * Remove um local do catálogo.
     * Ação permitida apenas ao autor original ou a administradores.
     */
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var place = await context.Places
            .SingleOrDefaultAsync(existingPlace => existingPlace.Id == id, cancellationToken);

        if (place is null)
            return PlaceErrors.NotFound;

        /** Bloqueia exclusão por usuários terceiros. */
        var isOwner = place.AuthorId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return PlaceErrors.NotOwner;

        context.Places.Remove(place);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
