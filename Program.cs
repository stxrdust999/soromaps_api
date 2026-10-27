using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using soromaps_api.Data.Seed;
using soromaps_api.Extensions;
using soromaps_api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomPersistence(builder.Configuration);
builder.Services.AddCustomOptions(builder.Configuration);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomRateLimiting();
builder.Services.AddApplicationServices();

builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Soromaps API",
            Version = "v1",
            Description = "API oficial da plataforma Soromaps — Mapeamento urbano e colaborativo de Sorocaba."
        };

        var components = document.Components ??= new OpenApiComponents();
        var securitySchemes = components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        securitySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT de acesso gerado no endpoint de login."
        });

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var authAttributes = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Authorization.IAuthorizeData>();

        var allowAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Authorization.IAllowAnonymous>();

        if (authAttributes.Any() && !allowAnonymous.Any())
        {
            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>()
            };

            var security = operation.Security ??= new List<OpenApiSecurityRequirement>();
            security.Add(requirement);
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
    return;
}

if (args.Any(arg => arg is "--import-places" or "--scrape-places" or "--import-osm"))
{
    using var scope = app.Services.CreateScope();
    var scraper = scope.ServiceProvider.GetRequiredService<IOsmPlaceScraperService>();
    var approveImmediately = args.Contains("--approve");
    await scraper.ScrapeAndImportAsync(approveImmediately);
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseRouting();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}
else
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
