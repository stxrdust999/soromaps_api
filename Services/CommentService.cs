using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por comentários em avaliações de locais.
 * Valida existência da avaliação-pai, autoria do comentário e paginação.
 */
public sealed class CommentService(AppDbContext context, ICurrentUserService currentUser) : ICommentService
{
    /**
     * Lista comentários de uma avaliação de forma paginada e cronológica.
     */
    public async Task<Result<PagedResponseDto<CommentResponseDto>>> ListByReviewIdAsync(
        Guid reviewId,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        return await context.Comments
            .AsNoTracking()
            .Where(comment => comment.ReviewId == reviewId)
            .OrderBy(comment => comment.CreatedAt)
            .Select(CommentMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Adiciona um comentário em uma avaliação existente vinculado ao usuário autenticado.
     */
    public async Task<Result<CommentResponseDto>> CreateAsync(CreateCommentDto request, CancellationToken cancellationToken = default)
    {
        /** Confere se a avaliação-alvo existe no banco antes de criar o vínculo. */
        var reviewExists = await context.Reviews
            .AnyAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (!reviewExists)
            return CommentErrors.ReviewNotFound;

        var comment = new Comment
        {
            ReviewId = request.ReviewId,
            UserId = currentUser.UserId,
            Content = request.Content.Trim()
        };

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        /** Carrega os dados do autor recém-associado para formatar a resposta com o resumo seguro do perfil. */
        await context.Entry(comment).Reference(c => c.User).LoadAsync(cancellationToken);

        return comment.ToResponse();
    }

    /**
     * Atualiza o conteúdo de um comentário existente.
     * Restrito ao autor original ou administradores.
     */
    public async Task<Result<CommentResponseDto>> UpdateAsync(Guid id, UpdateCommentDto request, CancellationToken cancellationToken = default)
    {
        var comment = await context.Comments
            .Include(existingComment => existingComment.User)
            .SingleOrDefaultAsync(existingComment => existingComment.Id == id, cancellationToken);

        if (comment is null)
            return CommentErrors.NotFound;

        /** Verifica posse do comentário para impedir edição não autorizada. */
        var isOwner = comment.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return CommentErrors.NotOwner;

        comment.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return comment.ToResponse();
    }

    /**
     * Exclui um comentário do sistema.
     * Restrito ao autor original ou administradores.
     */
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await context.Comments
            .SingleOrDefaultAsync(existingComment => existingComment.Id == id, cancellationToken);

        if (comment is null)
            return CommentErrors.NotFound;

        /** Verifica posse do comentário para impedir exclusão por terceiros. */
        var isOwner = comment.UserId == currentUser.UserId;
        var isAdmin = currentUser.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
            return CommentErrors.NotOwner;

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
