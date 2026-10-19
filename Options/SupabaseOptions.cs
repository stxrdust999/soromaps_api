namespace soromaps_api.Options;

public sealed class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; init; } = string.Empty;

    public string ServiceRoleKey { get; init; } = string.Empty;

    public string PlacesBucket { get; init; } = "soromaps_places";

    public string AvatarsBucket { get; init; } = "soromaps_pfps";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(ServiceRoleKey);
}
