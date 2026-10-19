namespace soromaps_api.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public string AdminName { get; init; } = "Administrador Soromaps";
}
