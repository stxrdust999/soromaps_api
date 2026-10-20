using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class ReviewErrors
{
    public static readonly Error NotFound = new(
        "review.not_found",
        "Avaliação não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error PlaceNotFound = new(
        "review.place_not_found",
        "O local informado não existe ou não está aprovado para receber avaliações.",
        ErrorType.Validation
    );

    public static readonly Error AlreadyExists = new(
        "review.already_exists",
        "Você já publicou uma avaliação para este local. Edite sua avaliação anterior se desejar alterá-la.",
        ErrorType.Conflict
    );

    public static readonly Error NotOwner = new(
        "review.not_owner",
        "Apenas o autor ou um administrador pode alterar ou excluir esta avaliação.",
        ErrorType.Forbidden
    );

    public static readonly Error CannotVoteOwn = new(
        "review.cannot_vote_own",
        "Você não pode marcar sua própria avaliação como útil.",
        ErrorType.Conflict
    );

    public static readonly Error NotVoted = new(
        "review.not_voted",
        "Você ainda não havia marcado esta avaliação como útil.",
        ErrorType.NotFound
    );
}
