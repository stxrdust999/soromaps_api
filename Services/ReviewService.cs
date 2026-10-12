using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pela gestão de avaliações de locais.
 * Aplica a regra de negócio de avaliação única por usuário em cada lugar e valida autoria.
 */
public sealed class ReviewService(AppDbContext context, ICurrentUserService currentUser) : IReviewService
{
    /**
     * Lista avaliações de um local específico de forma paginada e ordenada por data decrescente.
     */
    public async Task<Result<PagedResponseDto<ReviewResponseDto>>> ListByPlaceIdAsync(
        Guid placeId,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        return await context.Reviews
            .AsNoTracking()
            .Where(review => review.PlaceId == placeId)
            .OrderByDescending(review => review.CreatedAt)
            .Select(ReviewMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Cria uma nova avaliação para um local aprovado.
     * Impede avaliações em locais pendentes/inexistentes e duplicidade de review pelo mesmo autor.
     */
    public async Task<Result<ReviewResponseDto>> CreateAsync(CreateReviewDto request, CancellationToken cancellationToken = default)
    {
        /** Confere se o local existe e está aprovado para receber avaliações públicas. */
        var place = await context.Places
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == request.PlaceId, cancellationToken);

        if (place is null || place.Status != PlaceStatus.Approved)
            return ReviewErrors.PlaceNotFound;

        var currentUserId = currentUser.UserId;

        /** Verifica preventivamente na aplicação se o usuário já avaliou este lugar. */
        var alreadyReviewed = await context.Reviews
            .AnyAsync(r => r.PlaceId == request.PlaceId && r.UserId == currentUserId, cancellationToken);

        if (alreadyReviewed)
            return ReviewErrors.AlreadyExists;

        /** Instancia a avaliação com a nota e comentário fornecidos. */
        var review = new Review
        {
            PlaceId = request.PlaceId,
            UserId = currentUserId,
            Content = request.Content.Trim(),
            Rating = request.Rating
        };

        /** Persiste capturando eventual race condition contra o índice único do PostgreSQL. */
        try
        {
            context.Reviews.Add(review);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return ReviewErrors.AlreadyExists;
        }

        /** Carrega a navegação do autor para compor a resposta com o resumo seguro do perfil. */
        await context.Entry(review).Reference(r => r.User).LoadAsync(cancellationToken);

        return review.ToResponse();
    }

    /**
     * Atualiza o conteúdo ou nota de uma avaliação existente.
     * Restrito ao autor da avaliação ou administradores.
     */
    public async Task<Result<ReviewResponseDto>> UpdateAsync(Guid id, UpdateReviewDto request, CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews
            .Include(existingReview => existingReview.User)
            .SingleOrDefaultAsync(existingReview => existingReview.Id == id, cancellationToken);

        if (review is null)
            return ReviewErrors.NotFound;

        /** Verifica posse da avaliação para evitar adulteração de feedback por terceiros. */
        var isOwner = review.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return ReviewErrors.NotOwner;

        review.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return review.ToResponse();
    }

    /**
     * Exclui uma avaliação.
     * Restrito ao autor da avaliação ou administradores.
     */
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews
            .SingleOrDefaultAsync(existingReview => existingReview.Id == id, cancellationToken);

        if (review is null)
            return ReviewErrors.NotFound;

        /** Verifica posse da avaliação antes da remoção definitiva. */
        var isOwner = review.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return ReviewErrors.NotOwner;

        context.Reviews.Remove(review);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
