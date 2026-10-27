using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.Models;
using soromaps_api.Models.Enums;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

/**
 * Serviço responsável por minerar locais de interesse público e comercial em Sorocaba a partir da Overpass API do OpenStreetMap,
 * aplicando regras de deduplicação espacial/nominal e categorização automática.
 */
public sealed class OsmPlaceScraperService(
    HttpClient httpClient,
    AppDbContext context,
    ILogger<OsmPlaceScraperService> logger) : IOsmPlaceScraperService
{
    private const string OverpassEndpoint = "https://overpass-api.de/api/interpreter";

    private const string OverpassQuery = @"[out:json][timeout:30];
(
  node[amenity=cafe](-23.58,-47.55,-23.42,-47.38);
  way[amenity=cafe](-23.58,-47.55,-23.42,-47.38);
  node[amenity=restaurant](-23.58,-47.55,-23.42,-47.38);
  way[amenity=restaurant](-23.58,-47.55,-23.42,-47.38);
  node[amenity=bar](-23.58,-47.55,-23.42,-47.38);
  way[amenity=bar](-23.58,-47.55,-23.42,-47.38);
  node[leisure=park](-23.58,-47.55,-23.42,-47.38);
  way[leisure=park](-23.58,-47.55,-23.42,-47.38);
  node[tourism=museum](-23.58,-47.55,-23.42,-47.38);
  way[tourism=museum](-23.58,-47.55,-23.42,-47.38);
  node[historic](-23.58,-47.55,-23.42,-47.38);
  way[historic](-23.58,-47.55,-23.42,-47.38);
);
out center tags;";

    /**
     * Consulta a Overpass API com fallback resiliente para o Nominatim OSM, processa nós e vias retornados, descarta duplicatas e cadastra os novos pontos na base de dados.
     */
    public async Task<int> ScrapeAndImportAsync(bool approveImmediately = false, CancellationToken cancellationToken = default)
    {
        if (!httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SoromapsCommunityApp/1.0 (admin@soromaps.com)");

        var previouslySyncedPlaces = await context.Places
            .Where(place => place.Status == PlaceStatus.Approved &&
                            (place.Description.Contains("Importado via OpenStreetMap") ||
                             place.About.Contains("Ponto de interesse mapeado")))
            .ToListAsync(cancellationToken);

        if (previouslySyncedPlaces.Count > 0)
        {
            foreach (var syncedPlace in previouslySyncedPlaces)
            {
                syncedPlace.Status = PlaceStatus.Pending;
                syncedPlace.AuthorId = null;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        var activeCategories = await context.Categories
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ToListAsync(cancellationToken);

        if (activeCategories.Count == 0)
        {
            logger.LogWarning("Nenhuma categoria ativa encontrada para vincular aos locais importados.");
            return 0;
        }

        var fallbackCategory = activeCategories[0];
        var categoriesBySlug = activeCategories.ToDictionary(category => category.Slug, StringComparer.OrdinalIgnoreCase);

        var existingPlaces = await context.Places
            .AsNoTracking()
            .Select(place => new ExistingPlaceRecord(place.Name.Trim().ToLowerInvariant(), place.Latitude, place.Longitude))
            .ToListAsync(cancellationToken);

        var rawItems = await FetchPlacesFromOverpassAsync(cancellationToken);

        if (rawItems.Count == 0)
        {
            logger.LogInformation("Overpass indisponível ou vazio. Acionando fallback oficial do Nominatim OpenStreetMap para Sorocaba...");
            rawItems = await FetchPlacesFromNominatimAsync(cancellationToken);
        }

        var placesToInsert = new List<Place>();
        var totalMined = rawItems.Count;
        var placeStatus = approveImmediately ? PlaceStatus.Approved : PlaceStatus.Pending;

        foreach (var item in rawItems)
        {
            var name = item.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (name.Length > 120)
                name = name[..120].Trim();

            var normalizedName = name.ToLowerInvariant();
            var isDuplicate = existingPlaces.Any(existingPlace =>
                existingPlace.NormalizedName == normalizedName ||
                (Math.Abs(existingPlace.Latitude - item.Latitude) < 0.0003 && Math.Abs(existingPlace.Longitude - item.Longitude) < 0.0003));

            if (isDuplicate)
                continue;

            var category = !string.IsNullOrWhiteSpace(item.CategorySlug) && categoriesBySlug.TryGetValue(item.CategorySlug, out var matchedCategory)
                ? matchedCategory
                : fallbackCategory;

            var neighborhood = !string.IsNullOrWhiteSpace(item.Neighborhood) ? item.Neighborhood : "Sorocaba";
            if (neighborhood.Length > 60)
                neighborhood = neighborhood[..60].Trim();

            var descriptionText = item.Description;

            var about = !string.IsNullOrWhiteSpace(descriptionText)
                ? (descriptionText.Length > 280 ? descriptionText[..277].Trim() + "..." : descriptionText)
                : $"{name} em Sorocaba ({category.Name}). Ponto capturado via sincronização externa (OpenStreetMap).";

            if (about.Length > 280)
                about = about[..280].Trim();

            var syncSuffix = " [Sincronizado via OpenStreetMap]";
            var description = !string.IsNullOrWhiteSpace(descriptionText)
                ? (descriptionText.Length + syncSuffix.Length > 2000
                    ? descriptionText[..(2000 - syncSuffix.Length - 3)].Trim() + "..." + syncSuffix
                    : descriptionText + syncSuffix)
                : $"Ponto de interesse urbano ({category.Name}) localizado em {neighborhood}, Sorocaba. Capturado automaticamente via sincronização com API externa (OpenStreetMap) para curadoria da moderação.";

            if (description.Length > 2000)
                description = description[..2000].Trim();

            var newPlace = new Place
            {
                CategoryId = category.Id,
                AuthorId = null,
                Name = name,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                Neighborhood = neighborhood,
                About = about,
                Description = description,
                HasWifi = item.HasWifi,
                IsPetFriendly = item.IsPetFriendly,
                Status = placeStatus
            };

            placesToInsert.Add(newPlace);
            existingPlaces.Add(new ExistingPlaceRecord(normalizedName, item.Latitude, item.Longitude));
        }

        if (placesToInsert.Count > 0)
        {
            context.Places.AddRange(placesToInsert);
            await context.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Importação do OpenStreetMap concluída: {TotalMined} pontos minerados, {TotalImported} novos locais inseridos com status '{Status}'.",
            totalMined,
            placesToInsert.Count,
            placeStatus);

        return placesToInsert.Count;
    }

    private async Task<List<ScrapedRawItem>> FetchPlacesFromOverpassAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var requestContent = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("data", OverpassQuery)
            ]);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            using var response = await httpClient.PostAsync(OverpassEndpoint, requestContent, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Overpass API respondeu com status {StatusCode}.", (int)response.StatusCode);
                return [];
            }

            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var jsonDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            if (!jsonDocument.RootElement.TryGetProperty("elements", out var elementsProperty) || elementsProperty.ValueKind != JsonValueKind.Array)
                return [];

            var results = new List<ScrapedRawItem>();

            foreach (var element in elementsProperty.EnumerateArray())
            {
                if (!TryExtractCoordinates(element, out var latitude, out var longitude))
                    continue;

                if (!element.TryGetProperty("tags", out var tagsProperty) || tagsProperty.ValueKind != JsonValueKind.Object)
                    continue;

                if (!tagsProperty.TryGetProperty("name", out var nameProperty))
                    continue;

                var name = nameProperty.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var categorySlug = ResolveCategorySlug(tagsProperty);
                var neighborhood = ExtractNeighborhood(tagsProperty);
                var hasWifi = ExtractWifi(tagsProperty);
                var isPetFriendly = ExtractPetFriendly(tagsProperty);

                var description = tagsProperty.TryGetProperty("description", out var descriptionProperty)
                    ? descriptionProperty.GetString()?.Trim()
                    : null;

                results.Add(new ScrapedRawItem(name, latitude, longitude, categorySlug, neighborhood, hasWifi, isPetFriendly, description));
            }

            return results;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha na consulta Overpass: {Message}. Prosseguindo com fallback.", exception.Message);
            return [];
        }
    }

    private async Task<List<ScrapedRawItem>> FetchPlacesFromNominatimAsync(CancellationToken cancellationToken)
    {
        var queries = new (string Url, string CategorySlug)[]
        {
            ("https://nominatim.openstreetmap.org/search?city=Sorocaba&amenity=cafe&format=json&limit=30", "cafeterias"),
            ("https://nominatim.openstreetmap.org/search?q=parque+Sorocaba&format=json&limit=30", "parques"),
            ("https://nominatim.openstreetmap.org/search?city=Sorocaba&amenity=restaurant&format=json&limit=30", "gastronomia"),
            ("https://nominatim.openstreetmap.org/search?city=Sorocaba&amenity=bar&format=json&limit=25", "gastronomia"),
            ("https://nominatim.openstreetmap.org/search?q=museu+Sorocaba&format=json&limit=20", "cultura"),
            ("https://nominatim.openstreetmap.org/search?q=teatro+Sorocaba&format=json&limit=15", "cultura")
        };

        var items = new List<ScrapedRawItem>();

        foreach (var (url, categorySlug) in queries)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    continue;

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    if (!element.TryGetProperty("name", out var nameProperty))
                        continue;

                    var name = nameProperty.GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (!element.TryGetProperty("lat", out var latProperty) || !element.TryGetProperty("lon", out var lonProperty))
                        continue;

                    var latStr = latProperty.GetString();
                    var lonStr = lonProperty.GetString();

                    if (!double.TryParse(latStr, System.Globalization.CultureInfo.InvariantCulture, out var latitude) ||
                        !double.TryParse(lonStr, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
                        continue;

                    var displayName = element.TryGetProperty("display_name", out var displayProperty)
                        ? displayProperty.GetString()
                        : null;

                    var neighborhood = ExtractNeighborhoodFromDisplayName(displayName);

                    items.Add(new ScrapedRawItem(
                        Name: name,
                        Latitude: latitude,
                        Longitude: longitude,
                        CategorySlug: categorySlug,
                        Neighborhood: neighborhood,
                        HasWifi: categorySlug == "cafeterias",
                        IsPetFriendly: categorySlug == "parques",
                        Description: null));
                }

                // Respeita política de requisições do Nominatim
                await Task.Delay(400, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning("Falha ao buscar locais do Nominatim ({Url}): {Message}", url, exception.Message);
            }
        }

        return items;
    }

    private static string ExtractNeighborhoodFromDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "Sorocaba";

        var parts = displayName.Split(',').Select(part => part.Trim()).ToArray();
        // O bairro no Nominatim geralmente aparece antes de 'Sorocaba'
        var sorocabaIndex = Array.FindIndex(parts, p => p.Equals("Sorocaba", StringComparison.OrdinalIgnoreCase));

        if (sorocabaIndex > 1)
            return parts[sorocabaIndex - 1];

        if (parts.Length > 2)
            return parts[1];

        return "Sorocaba";
    }

    private static string? ResolveCategorySlug(JsonElement tags)
    {
        if (tags.TryGetProperty("amenity", out var amenityProperty))
        {
            var amenity = amenityProperty.GetString()?.ToLowerInvariant();
            if (amenity == "cafe")
                return "cafeterias";
            if (amenity is "restaurant" or "bar")
                return "gastronomia";
        }

        if (tags.TryGetProperty("leisure", out var leisureProperty))
        {
            var leisure = leisureProperty.GetString()?.ToLowerInvariant();
            if (leisure == "park")
                return "parques";
        }

        if (tags.TryGetProperty("tourism", out var tourismProperty))
        {
            var tourism = tourismProperty.GetString()?.ToLowerInvariant();
            if (tourism == "museum")
                return "cultura";
        }

        if (tags.TryGetProperty("historic", out var historicProperty))
        {
            var historic = historicProperty.GetString()?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(historic) && historic != "no")
                return "cultura";
        }

        return null;
    }

    private static bool TryExtractCoordinates(JsonElement element, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        if (!element.TryGetProperty("type", out var typeProperty))
            return false;

        var elementType = typeProperty.GetString();

        if (elementType == "node")
        {
            if (element.TryGetProperty("lat", out var latitudeProperty) &&
                element.TryGetProperty("lon", out var longitudeProperty) &&
                latitudeProperty.TryGetDouble(out latitude) &&
                longitudeProperty.TryGetDouble(out longitude))
                return true;

            return false;
        }

        if (elementType == "way")
        {
            if (element.TryGetProperty("center", out var centerProperty) &&
                centerProperty.ValueKind == JsonValueKind.Object &&
                centerProperty.TryGetProperty("lat", out var centerLatitudeProperty) &&
                centerProperty.TryGetProperty("lon", out var centerLongitudeProperty) &&
                centerLatitudeProperty.TryGetDouble(out latitude) &&
                centerLongitudeProperty.TryGetDouble(out longitude))
                return true;

            return false;
        }

        return false;
    }

    private static string ExtractNeighborhood(JsonElement tags)
    {
        var neighborhood = "Sorocaba";

        if (tags.TryGetProperty("addr:suburb", out var suburbProperty) && !string.IsNullOrWhiteSpace(suburbProperty.GetString()))
            neighborhood = suburbProperty.GetString()!.Trim();
        else if (tags.TryGetProperty("addr:neighbourhood", out var neighbourhoodProperty) && !string.IsNullOrWhiteSpace(neighbourhoodProperty.GetString()))
            neighborhood = neighbourhoodProperty.GetString()!.Trim();

        if (neighborhood.Length > 60)
            neighborhood = neighborhood[..60].Trim();

        return neighborhood;
    }

    private static bool ExtractWifi(JsonElement tags)
    {
        if (!tags.TryGetProperty("internet_access", out var internetAccessProperty))
            return false;

        var internetAccess = internetAccessProperty.GetString()?.ToLowerInvariant();
        return internetAccess is "wlan" or "yes";
    }

    private static bool ExtractPetFriendly(JsonElement tags)
    {
        if (!tags.TryGetProperty("dogs", out var dogsProperty))
            return false;

        var dogs = dogsProperty.GetString()?.ToLowerInvariant();
        return dogs is "yes" or "leashed";
    }

    private sealed record ExistingPlaceRecord(string NormalizedName, double Latitude, double Longitude);

    private sealed record ScrapedRawItem(
        string Name,
        double Latitude,
        double Longitude,
        string? CategorySlug,
        string? Neighborhood,
        bool HasWifi,
        bool IsPetFriendly,
        string? Description);
}
