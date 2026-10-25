using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using soromaps_api.Constants;
using soromaps_api.Data;
using soromaps_api.Data.Interceptors;
using soromaps_api.Data.Seed;
using soromaps_api.Options;
using soromaps_api.Services;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(interceptor)
                .ReplaceService<IMigrationsSqlGenerator, RlsMigrationsSqlGenerator>();
        });

        return services;
    }

    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.SectionName);

        services.AddOptions<SeedOptions>()
            .BindConfiguration(SeedOptions.SectionName);

        services.AddOptions<SupabaseOptions>()
            .BindConfiguration(SupabaseOptions.SectionName);

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured");

        var issuer = configuration["Jwt:Issuer"] ?? "soromaps_api";
        var audience = configuration["Jwt:Audience"] ?? "soromaps_web";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole("admin"));
        });

        return services;
    }

    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Login, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous_ip",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }
                )
            );

            options.AddPolicy(RateLimitPolicies.Register, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous_ip",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }
                )
            );

            options.AddPolicy(RateLimitPolicies.Refresh, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous_ip",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }
                )
            );
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();
        services.AddHttpClient<IOsmPlaceScraperService, OsmPlaceScraperService>();

        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IPlaceService, PlaceService>();
        services.AddScoped<IModerationService, ModerationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAdminReviewService, AdminReviewService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAchievementEvaluatorService, AchievementEvaluatorService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        services.AddHostedService<SessionCleanupService>();

        return services;
    }
}
