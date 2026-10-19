using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class PlaceErrors
{
    public static readonly Error NotFound = new(
        "place.not_found",
        "Lugar não encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error NotOwner = new(
        "place.not_owner",
        "Apenas o autor ou um administrador pode alterar ou excluir este lugar.",
        ErrorType.Forbidden
    );

    public static readonly Error CategoryNotFound = new(
        "place.category_not_found",
        "A categoria informada não existe.",
        ErrorType.Validation
    );

    public static readonly Error InvalidBoundingBox = new(
        "place.invalid_bbox",
        "O parâmetro bbox deve conter 4 coordenadas separadas por vírgula (minLng,minLat,maxLng,maxLat).",
        ErrorType.Validation
    );

    public static readonly Error PhotoNotFound = new(
        "place.photo_not_found",
        "Foto do local não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error StorageNotConfigured = new(
        "storage.not_configured",
        "O serviço de armazenamento de imagens não está configurado no servidor.",
        ErrorType.Validation
    );

    public static readonly Error StorageUploadFailed = new(
        "storage.upload_failed",
        "Não foi possível gerar a URL de upload no serviço de armazenamento.",
        ErrorType.Validation
    );
}
