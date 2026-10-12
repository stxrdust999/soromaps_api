using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class ReviewErrors
{
    public static readonly Error NotFound = new(
        "review.not_found",
        "Avaliação não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error NotOwner = new(
        "review.not_owner",
        "Apenas o autor ou um administrador pode alterar ou excluir esta avaliação.",
        ErrorType.Forbidden
    );

    public static readonly Error AlreadyExists = new(
        "review.already_exists",
        "Você já avaliou este lugar.",
        ErrorType.Conflict
    );

    public static readonly Error PlaceNotFound = new(
        "review.place_not_found",
        "O lugar informado para avaliação não existe ou ainda não foi aprovado.",
        ErrorType.Validation
    );
}
