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
}
