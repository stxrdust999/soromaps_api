using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using soromaps_api.Data;
using soromaps_api.DTOs.Auth;
using soromaps_api.Errors;
using soromaps_api.Mappings;
using soromaps_api.Models;
using soromaps_api.Options;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável pela emissão de credenciais, validação de login e ciclo de vida de sessões.
 * Implementa rotação segura de refresh tokens (RTR), mitigação contra timing attacks e detecção de reuso.
 */
public sealed class AuthService(
    AppDbContext context,
    ITokenService tokenService,
    ICurrentUserService currentUser,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthOptions> authOptions,
    TimeProvider timeProvider) : IAuthService
{
    private readonly JwtOptions jwt = jwtOptions.Value;
    private readonly AuthOptions auth = authOptions.Value;

    /**
     * Autentica o usuário validando senha em BCrypt contra ataques de enumeração por tempo (timing attacks).
     * Emite um par de Access Token (JWT curto) e Refresh Token (opaco, 7 dias).
     */
    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequestDto request,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        /** Normaliza busca de e-mail e executa verificação de hash mesmo quando usuário não existe. */
        var user = await context.Users.SingleOrDefaultAsync(
            existingUser => existingUser.Email == normalizedEmail,
            cancellationToken
        );

        /**
         * Executa BCrypt.Verify usando DummyHash pré-calculado se o e-mail não for encontrado.
         * Garante tempo constante de resposta (~100ms) impedindo descobrir contas existentes por latência.
         */
        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user?.PasswordHash ?? auth.DummyHash
        );

        if (user == null || !passwordValid)
            return AuthErrors.InvalidCredentials;

        /** Cria a sessão e retorna o token de acesso acompanhado do refresh token raw. */
        return await IssueTokensAsync(user, userAgent, cancellationToken);
    }

    /**
     * Renova o par de tokens aplicando o padrão Refresh Token Rotation (RTR).
     * Revoga a sessão antiga imediatamente e detecta reuso fraudulento de tokens já invalidados.
     */
    public async Task<Result<AuthResponseDto>> RefreshAsync(
        RefreshTokenRequestDto request,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        /** Obtém o hash SHA-256 do token recebido para buscar a sessão no banco sem expor o token raw. */
        var tokenHash = tokenService.HashToken(request.RefreshToken);

        var session = await context.Sessions
            .Include(existingSession => existingSession.User)
            .SingleOrDefaultAsync(existingSession => existingSession.TokenHash == tokenHash, cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        /**
         * Detecção de Reuso de Token (Ataque Man-in-the-Middle):
         * Se o token apresentado já foi revogado anteriormente, revoga todas as sessões do usuário.
         */
        if (session != null && session.RevokedAt != null)
        {
            await context.Sessions
                .Where(existingSession => existingSession.UserId == session.UserId && existingSession.RevokedAt == null)
                .ExecuteUpdateAsync(setter => setter.SetProperty(s => s.RevokedAt, now), cancellationToken);

            return AuthErrors.InvalidRefreshToken;
        }

        if (session == null || session.ExpiresAt <= now)
            return AuthErrors.InvalidRefreshToken;

        /** Invalida a sessão atual para que o mesmo refresh token não seja reutilizado. */
        session.RevokedAt = now;

        /** Gera um novo par de tokens de acesso e refresh. */
        var newAccessToken = tokenService.GenerateAccessToken(session.User);
        var newRawRefreshToken = tokenService.GenerateRefreshToken();
        var newTokenHash = tokenService.HashToken(newRawRefreshToken);

        var newSession = new Session
        {
            UserId = session.User.Id,
            TokenHash = newTokenHash,
            ExpiresAt = now.AddDays(jwt.RefreshTokenLifetimeDays),
            UserAgent = userAgent,
            CreatedAt = now
        };

        context.Sessions.Add(newSession);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            newAccessToken,
            newRawRefreshToken,
            jwt.AccessTokenLifetimeMinutes * 60,
            session.User.ToResponse()
        );
    }

    /**
     * Encerra a sessão atual do dispositivo marcando o refresh token correspondente como revogado.
     */
    public async Task<Result> LogoutAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.HashToken(request.RefreshToken);
        var currentUserId = currentUser.UserId;

        /** Busca a sessão ativa garantindo que pertence ao usuário autenticado no momento. */
        var session = await context.Sessions
            .SingleOrDefaultAsync(
                existingSession => existingSession.TokenHash == tokenHash && existingSession.UserId == currentUserId,
                cancellationToken
            );

        if (session != null && session.RevokedAt == null)
        {
            session.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    /**
     * Encerra todas as sessões ativas do usuário em todos os dispositivos simultaneamente.
     * Utiliza ExecuteUpdateAsync para executar um único comando SQL em lote no PostgreSQL.
     */
    public async Task<Result> LogoutAllAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        if (currentUserId == Guid.Empty)
            return Result.Success();

        var now = timeProvider.GetUtcNow().UtcDateTime;

        /** Executa UPDATE em lote sem carregar as entidades em memória nem sobrecarregar o ChangeTracker. */
        await context.Sessions
            .Where(session => session.UserId == currentUserId && session.RevokedAt == null)
            .ExecuteUpdateAsync(setter => setter.SetProperty(s => s.RevokedAt, now), cancellationToken);

        return Result.Success();
    }

    /**
     * Método auxiliar privado para emissão inicial de sessão e tokens.
     */
    private async Task<AuthResponseDto> IssueTokensAsync(
        User user,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var accessToken = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var tokenHash = tokenService.HashToken(rawRefreshToken);

        var session = new Session
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddDays(jwt.RefreshTokenLifetimeDays),
            UserAgent = userAgent,
            CreatedAt = now
        };

        context.Sessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            rawRefreshToken,
            jwt.AccessTokenLifetimeMinutes * 60,
            user.ToResponse()
        );
    }
}
