using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feedback;
using soromaps_api.Extensions;
using soromaps_api.Models;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço de registro e consulta de feedbacks da comunidade de exploradores.
 */
public sealed class FeedbackService(
    AppDbContext context,
    ICurrentUserService currentUser,
    TimeProvider timeProvider) : IFeedbackService
{
    /**
     * Registra o feedback do usuário com sua classificação e rota de origem.
     */
    public async Task<Result<FeedbackResponseDto>> CreateAsync(CreateFeedbackDto request, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var feedback = new Feedback
        {
            UserId = userId,
            Category = request.Category,
            OriginPath = string.IsNullOrWhiteSpace(request.OriginPath) ? null : request.OriginPath.Trim(),
            Message = request.Message.Trim(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        context.Feedbacks.Add(feedback);
        await context.SaveChangesAsync(cancellationToken);

        var author = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new AuthorSummaryDto(u.Id, u.Name, u.AvatarUrl))
            .SingleAsync(cancellationToken);

        return new FeedbackResponseDto(
            feedback.Id,
            author,
            feedback.Category,
            feedback.OriginPath,
            feedback.Message,
            feedback.CreatedAt
        );
    }

    /**
     * Lista de forma paginada os feedbacks enviados à plataforma.
     */
    public async Task<Result<PagedResponseDto<FeedbackResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default)
    {
        return await context.Feedbacks
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackResponseDto(
                f.Id,
                new AuthorSummaryDto(f.User.Id, f.User.Name, f.User.AvatarUrl),
                f.Category,
                f.OriginPath,
                f.Message,
                f.CreatedAt
            ))
            .ToPagedResponseAsync(pagination, cancellationToken);
    }
}
