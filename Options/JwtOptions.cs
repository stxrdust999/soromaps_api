using System.ComponentModel.DataAnnotations;

namespace soromaps_api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } = "soromaps_api";

    public string Audience { get; init; } = "soromaps_web";

    [Range(1, 60)]
    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    [Range(1, 30)]
    public int RefreshTokenLifetimeDays { get; init; } = 7;
}
