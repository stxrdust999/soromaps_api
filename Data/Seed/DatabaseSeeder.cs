using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Options;

namespace soromaps_api.Data.Seed;

public sealed class DatabaseSeeder(
    AppDbContext context,
    IOptions<SeedOptions> seedOptions,
    IHostEnvironment environment,
    ILogger<DatabaseSeeder> logger)
{
    private readonly SeedOptions _seedOptions = seedOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminUser = await SeedAdminAsync(cancellationToken);
        await SeedCatalogAsync(cancellationToken);

        if (environment.IsDevelopment())
            await SeedDevelopmentPlacesAsync(adminUser, cancellationToken);
    }

    private async Task<User?> SeedAdminAsync(CancellationToken cancellationToken)
    {
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(user => user.Role == UserRole.Admin, cancellationToken);

        if (existingAdmin is not null)
        {
            logger.LogInformation("Administrador já existente ({Email}).", existingAdmin.Email);
            return existingAdmin;
        }

        if (string.IsNullOrWhiteSpace(_seedOptions.AdminEmail) || string.IsNullOrWhiteSpace(_seedOptions.AdminPassword))
        {
            logger.LogWarning("Seed:AdminEmail ou Seed:AdminPassword não configurados. Pulando criação do primeiro administrador.");
            return null;
        }

        var admin = new User
        {
            Name = string.IsNullOrWhiteSpace(_seedOptions.AdminName) ? "Administrador Soromaps" : _seedOptions.AdminName.Trim(),
            Email = _seedOptions.AdminEmail.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(_seedOptions.AdminPassword, workFactor: 12),
            Role = UserRole.Admin,
            Neighborhood = "Centro",
            Biography = "Conta administrativa oficial da plataforma Soromaps."
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Primeiro administrador criado com sucesso ({Email}).", admin.Email);
        return admin;
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            var initialCategories = new List<Category>
            {
                new() { Name = "Cafeterias & Padarias", Slug = "cafeterias", Icon = "coffee", Color = "#D97706", SortOrder = 1, IsActive = true },
                new() { Name = "Parques & Áreas Verdes", Slug = "parques", Icon = "trees", Color = "#16A34A", SortOrder = 2, IsActive = true },
                new() { Name = "Gastronomia & Bares", Slug = "gastronomia", Icon = "utensils", Color = "#DC2626", SortOrder = 3, IsActive = true },
                new() { Name = "Cultura & História", Slug = "cultura", Icon = "landmark", Color = "#7C3AED", SortOrder = 4, IsActive = true },
                new() { Name = "Lazer & Esportes", Slug = "lazer", Icon = "bike", Color = "#2563EB", SortOrder = 5, IsActive = true },
                new() { Name = "Comércio Local", Slug = "comercio-local", Icon = "store", Color = "#0D9488", SortOrder = 6, IsActive = true }
            };

            context.Categories.AddRange(initialCategories);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Catálogo inicial de {Count} categorias populado.", initialCategories.Count);
        }

        if (!await context.Tags.AnyAsync(cancellationToken))
        {
            var initialTags = new List<Tag>
            {
                new() { Name = "Ao ar livre", Slug = "ao-ar-livre" },
                new() { Name = "Bom para trabalhar", Slug = "bom-para-trabalhar" },
                new() { Name = "Entrada gratuita", Slug = "entrada-gratuita" },
                new() { Name = "Para ir em família", Slug = "familia" },
                new() { Name = "Fim de tarde", Slug = "fim-de-tarde" },
                new() { Name = "Café especial", Slug = "cafe-especial" },
                new() { Name = "Acessível", Slug = "acessivel" },
                new() { Name = "Estacionamento fácil", Slug = "estacionamento-facil" }
            };

            context.Tags.AddRange(initialTags);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Catálogo inicial de {Count} tags populado.", initialTags.Count);
        }

        if (!await context.Achievements.AnyAsync(cancellationToken))
        {
            var initialAchievements = new List<Achievement>
            {
                new() { Name = "Primeiros Passos", Description = "Registrou a primeira visita em um local de Sorocaba." },
                new() { Name = "Crítico da Cidade", Description = "Publicou 3 avaliações detalhadas sobre locais visitados." },
                new() { Name = "Rota do Café", Description = "Visitou 5 cafeterias diferentes em Sorocaba." },
                new() { Name = "Cartógrafo Local", Description = "Sugeriu um novo ponto que foi aprovado pela curadoria." },
                new() { Name = "Desbravador de Bairros", Description = "Explorou lugares em pelo menos 5 bairros distintos." }
            };

            context.Achievements.AddRange(initialAchievements);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Catálogo inicial de {Count} conquistas populado.", initialAchievements.Count);
        }
    }

    private async Task SeedDevelopmentPlacesAsync(User? adminUser, CancellationToken cancellationToken)
    {
        if (await context.Places.AnyAsync(cancellationToken))
            return;

        var categories = await context.Categories.ToDictionaryAsync(category => category.Slug, cancellationToken);
        var tags = await context.Tags.ToDictionaryAsync(tag => tag.Slug, cancellationToken);

        var parkPlace = new Place
        {
            CategoryId = categories["parques"].Id,
            AuthorId = adminUser?.Id,
            Name = "Parque Carlos Alberto de Souza",
            Latitude = -23.5326,
            Longitude = -47.4656,
            Neighborhood = "Campolim",
            About = "Pista de caminhada, lagos e gramado amplo no coração do Campolim.",
            Description = "Um dos principais pontos de encontro ao ar livre da zona sul de Sorocaba, com pista de corrida, aparelhos de ginástica e feirinha gastronômica.",
            HasWifi = false,
            IsPetFriendly = true,
            BestTime = "Fim de tarde (16h30 às 18h30)",
            LocalSecret = "O pôr do sol visto da ponte próxima ao lago principal rende as melhores fotos.",
            Status = PlaceStatus.Approved
        };

        var coffeePlace = new Place
        {
            CategoryId = categories["cafeterias"].Id,
            AuthorId = adminUser?.Id,
            Name = "Cabocafé Torrefação Artesanal",
            Latitude = -23.5062,
            Longitude = -47.4559,
            Neighborhood = "Jardim Faculdade",
            About = "Cafés especiais torrados na casa com ambiente silencioso e tomadas.",
            Description = "Cafeteria focada em grãos especiais do interior paulista e sul de minas, métodos filtrados na hora e cardápio enxuto de fermentação natural.",
            HasWifi = true,
            IsPetFriendly = true,
            BestTime = "Manhãs de terça a sexta (9h às 11h30)",
            LocalSecret = "Peça o coado do grão sazonal acompanhado do pão de queijo na chapa.",
            Status = PlaceStatus.Approved
        };

        var culturalPlace = new Place
        {
            CategoryId = categories["cultura"].Id,
            AuthorId = adminUser?.Id,
            Name = "Museu da Estrada de Ferro Sorocabana",
            Latitude = -23.4979,
            Longitude = -47.4526,
            Neighborhood = "Centro",
            About = "Acervo histórico ferroviário em prédio clássico ao lado da antiga estação.",
            Description = "Espaço dedicado à memória da ferrovia que moldou o crescimento industrial de Sorocaba, com locomotivas, fotografias e documentos históricos.",
            HasWifi = false,
            IsPetFriendly = false,
            BestTime = "Sábados pela manhã (10h às 13h)",
            LocalSecret = "Combine a visita com uma caminhada pelo jardim externo ao lado da Casa do Turista.",
            Status = PlaceStatus.Approved
        };

        context.Places.AddRange(parkPlace, coffeePlace, culturalPlace);
        await context.SaveChangesAsync(cancellationToken);

        context.PlaceTags.AddRange(
            new PlaceTag { PlaceId = parkPlace.Id, TagId = tags["ao-ar-livre"].Id },
            new PlaceTag { PlaceId = parkPlace.Id, TagId = tags["entrada-gratuita"].Id },
            new PlaceTag { PlaceId = parkPlace.Id, TagId = tags["fim-de-tarde"].Id },
            new PlaceTag { PlaceId = coffeePlace.Id, TagId = tags["bom-para-trabalhar"].Id },
            new PlaceTag { PlaceId = coffeePlace.Id, TagId = tags["cafe-especial"].Id },
            new PlaceTag { PlaceId = culturalPlace.Id, TagId = tags["entrada-gratuita"].Id },
            new PlaceTag { PlaceId = culturalPlace.Id, TagId = tags["familia"].Id }
        );

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Locais de demonstração populados no ambiente de desenvolvimento.");
    }
}
