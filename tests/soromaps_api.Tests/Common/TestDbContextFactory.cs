namespace soromaps_api.Tests.Common;

using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;

/**
 * Fábrica de contextos de banco de dados em memória para testes unitários.
 * Cria instâncias isoladas com nome exclusivo por teste para assegurar independência de estado.
 */
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }
}
