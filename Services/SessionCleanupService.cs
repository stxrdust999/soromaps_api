using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;

namespace soromaps_api.Services;

public sealed class SessionCleanupService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<SessionCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSessionsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Erro ao executar limpeza periódica de sessões.");
            }

            await Task.Delay(Interval, timeProvider, stoppingToken);
        }
    }

    private async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = timeProvider.GetUtcNow().UtcDateTime.Subtract(RetentionPeriod);

        var deletedCount = await context.Sessions
            .Where(session => session.ExpiresAt < cutoff || (session.RevokedAt != null && session.RevokedAt < cutoff))
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
            logger.LogInformation("Limpeza de sessões concluída: {Count} registros removidos.", deletedCount);
    }
}
