using Microsoft.EntityFrameworkCore;
using Npgsql;
using soromaps_api.Data;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Users;
using soromaps_api.Errors;
using soromaps_api.Extensions;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pelo gerenciamento de usuários e perfis.
 * Centraliza cadastro com hash BCrypt, leitura segura de perfil (/me)
 * e sanitização de dados públicos para conformidade com a LGPD.
 */
public sealed class UserService(AppDbContext context, ICurrentUserService currentUser) : IUserService
{
    /**
     * Registra uma nova conta com papel padrão 'Explorer' e senha cifrada em BCrypt custo 12.
     * Trata concorrência no e-mail tanto por pré-checagem quanto capturando a unique constraint do Postgres.
     */
    public async Task<Result<UserResponseDto>> RegisterAsync(RegisterUserDto request, CancellationToken cancellationToken = default)
    {
        /** Normaliza o e-mail em minúsculas para buscas case-insensitive consistentes. */
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        /** Checagem prévia na aplicação para evitar overhead desnecessário de hashing se o e-mail já existir. */
        var emailExists = await context.Users
            .AnyAsync(existingUser => existingUser.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            return UserErrors.EmailTaken;

        /** Computa o hash com fator de trabalho 12 (~100-200ms de custo computacional defensivo). */
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.Explorer
        };

        /** Persiste o usuário protegendo contra corrida (race condition) no índice único de e-mail. */
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return UserErrors.EmailTaken;
        }

        return user.ToResponse();
    }

    /**
     * Retorna o perfil completo do usuário autenticado no token atual.
     * Utilizado pela rota protegida /api/users/me.
     */
    public async Task<Result<UserResponseDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        /** Obtém o ID do usuário diretamente dos claims do token JWT via CurrentUserService. */
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        return user.ToResponse();
    }

    /**
     * Busca o perfil público de um usuário por ID.
     * Omite dados sensíveis como e-mail ou sessões ativas para proteger a privacidade (LGPD).
     */
    public async Task<Result<PublicUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        /** Projeta apenas os campos públicos diretamente na query do banco de dados. */
        var profile = await context.Users
            .AsNoTracking()
            .Where(existingUser => existingUser.Id == userId)
            .Select(UserMappings.ToPublicProfileProjection)
            .SingleOrDefaultAsync(cancellationToken);

        if (profile is null)
            return UserErrors.NotFound;

        return profile;
    }

    /**
     * Lista usuários de forma paginada para painel administrativo.
     * Rota de acesso exclusivo para usuários com papel Admin.
     */
    public async Task<Result<PagedResponseDto<UserResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(existingUser => existingUser.Name)
            .Select(UserMappings.ToResponseProjection)
            .ToPagedResponseAsync(pagination, cancellationToken);
    }

    /**
     * Atualiza dados cadastrais do próprio usuário autenticado.
     * Impede qualquer tentativa de IDOR pois opera unicamente sobre o ID extraído do token.
     */
    public async Task<Result<UserResponseDto>> UpdateCurrentAsync(UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        /** Aplica apenas os campos não nulos informados no payload de atualização. */
        user.ApplyUpdate(request);
        await context.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    /**
     * Exclui a conta do usuário autenticado.
     * Aplica o direito ao esquecimento da LGPD: as sessões são apagadas em cascata
     * e os conteúdos públicos (lugares/avaliações) têm o author_id desvinculado (SetNull).
     */
    public async Task<Result> DeleteCurrentAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var user = await context.Users
            .SingleOrDefaultAsync(existingUser => existingUser.Id == currentUserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
