using System.Linq.Expressions;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre a entidade User e seus DTOs.
 * Fornece expressões de projeção para o EF Core traduzir em SQL e métodos de extensão para objetos em memória.
 */
public static class UserMappings
{
    /**
     * Expressão de projeção detalhada para perfil privado (/me) e rotas administrativas.
     * Gera uma query SQL trazendo os dados de auditoria e credenciais abertas como e-mail.
     * Utilizada em IQueryable para mapear diretamente no banco sem carregar a entidade completa na memória.
     */
    public static readonly Expression<Func<User, UserResponseDto>> ToResponseProjection = user =>
        new UserResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString().ToLowerInvariant(),
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    /**
     * Expressão de projeção pública para perfis visíveis por outros usuários.
     * Gera SQL que omite explicitamente o e-mail, hash de senha e listas de sessões no banco,
     * garantindo conformidade com o princípio de necessidade da LGPD e menor uso de banda.
     */
    public static readonly Expression<Func<User, PublicUserProfileDto>> ToPublicProfileProjection = user =>
        new PublicUserProfileDto(
            user.Id,
            user.Name,
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    /**
     * Expressão de projeção resumida do autor.
     * Utilizada para acoplar os dados essenciais do criador de um lugar, review ou comentário
     * sem carregar o perfil completo do usuário.
     */
    public static readonly Expression<Func<User, AuthorSummaryDto>> ToAuthorSummaryProjection = user =>
        new AuthorSummaryDto(
            user.Id,
            user.Name,
            user.AvatarUrl
        );

    /**
     * Mapeia uma entidade User já carregada na memória para o DTO de resposta detalhado.
     * Usado após operações de persistência direta (ex: Register ou UpdateCurrent).
     */
    public static UserResponseDto ToResponse(this User user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString().ToLowerInvariant(),
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    /**
     * Mapeia uma entidade User já carregada na memória para o DTO de perfil público.
     */
    public static PublicUserProfileDto ToPublicProfile(this User user) =>
        new(
            user.Id,
            user.Name,
            user.AvatarUrl,
            user.Biography,
            user.Neighborhood,
            user.CreatedAt
        );

    /**
     * Mapeia uma entidade User já carregada na memória para o DTO resumido do autor.
     */
    public static AuthorSummaryDto ToAuthorSummary(this User user) =>
        new(
            user.Id,
            user.Name,
            user.AvatarUrl
        );

    /**
     * Aplica alterações parciais vindas de UpdateUserDto sobre a entidade rastreada,
     * ignorando propriedades nulas e aplicando sanitização (Trim) nos campos de texto.
     */
    public static void ApplyUpdate(this User user, UpdateUserDto request)
    {
        if (request.Name is not null)
            user.Name = request.Name.Trim();

        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl.Trim();

        if (request.Biography is not null)
            user.Biography = request.Biography.Trim();

        if (request.Neighborhood is not null)
            user.Neighborhood = request.Neighborhood.Trim();
    }
}
