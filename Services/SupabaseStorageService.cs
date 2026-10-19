using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using soromaps_api.DTOs.Common;
using soromaps_api.Errors;
using soromaps_api.Options;
using soromaps_api.Results;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Services;

public sealed class SupabaseStorageService(
    HttpClient httpClient,
    IOptions<SupabaseOptions> supabaseOptions) : ISupabaseStorageService
{
    private readonly SupabaseOptions _options = supabaseOptions.Value;

    public Task<Result<SignedUploadUrlDto>> CreatePlacePhotoUploadUrlAsync(
        Guid placeId,
        string? fileExtension = "webp",
        CancellationToken cancellationToken = default)
    {
        var sanitizedExtension = SanitizeExtension(fileExtension);
        var storagePath = $"places/{placeId}/{Guid.NewGuid():N}.{sanitizedExtension}";

        return CreateSignedUploadUrlInternalAsync(_options.PlacesBucket, storagePath, cancellationToken);
    }

    public Task<Result<SignedUploadUrlDto>> CreateAvatarUploadUrlAsync(
        Guid userId,
        string? fileExtension = "webp",
        CancellationToken cancellationToken = default)
    {
        var sanitizedExtension = SanitizeExtension(fileExtension);
        var storagePath = $"users/{userId}/{Guid.NewGuid():N}.{sanitizedExtension}";

        return CreateSignedUploadUrlInternalAsync(_options.AvatarsBucket, storagePath, cancellationToken);
    }

    public async Task DeleteObjectAsync(
        string bucket,
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured || string.IsNullOrWhiteSpace(storagePath))
            return;

        var baseUrl = _options.Url.TrimEnd('/');
        var requestUri = $"{baseUrl}/storage/v1/object/{bucket}/{storagePath.TrimStart('/')}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("apikey", _options.ServiceRoleKey);

        try
        {
            await httpClient.SendAsync(request, cancellationToken);
        }
        catch
        {
            // Falha na limpeza de arquivo órfão do storage não deve abortar a operação principal no banco.
        }
    }

    private async Task<Result<SignedUploadUrlDto>> CreateSignedUploadUrlInternalAsync(
        string bucket,
        string storagePath,
        CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
            return PlaceErrors.StorageNotConfigured;

        var baseUrl = _options.Url.TrimEnd('/');
        var signEndpoint = $"{baseUrl}/storage/v1/object/upload/sign/{bucket}/{storagePath}";

        using var request = new HttpRequestMessage(HttpMethod.Post, signEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("apikey", _options.ServiceRoleKey);
        request.Content = JsonContent.Create(new { });

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch
        {
            return PlaceErrors.StorageUploadFailed;
        }

        if (!response.IsSuccessStatusCode)
            return PlaceErrors.StorageUploadFailed;

        var payload = await response.Content.ReadFromJsonAsync<SupabaseSignUploadResponse>(cancellationToken: cancellationToken);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Url))
            return PlaceErrors.StorageUploadFailed;

        var relativeSignedUrl = payload.Url.StartsWith('/') ? payload.Url : $"/{payload.Url}";
        var fullUploadUrl = relativeSignedUrl.StartsWith("/storage/v1")
            ? $"{baseUrl}{relativeSignedUrl}"
            : $"{baseUrl}/storage/v1{relativeSignedUrl}";

        var publicUrl = $"{baseUrl}/storage/v1/object/public/{bucket}/{storagePath}";

        return new SignedUploadUrlDto(
            fullUploadUrl,
            publicUrl,
            storagePath,
            bucket
        );
    }

    private static string SanitizeExtension(string? fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
            return "webp";

        var cleaned = fileExtension.Trim().TrimStart('.').ToLowerInvariant();
        return cleaned switch
        {
            "jpg" or "jpeg" or "png" or "webp" or "avif" => cleaned,
            _ => "webp"
        };
    }

    private sealed record SupabaseSignUploadResponse(
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("token")] string? Token
    );
}
