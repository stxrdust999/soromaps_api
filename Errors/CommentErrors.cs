using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new(
        "comment.not_found",
        "Comentário não encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error NotOwner = new(
        "comment.not_owner",
        "Apenas o autor ou um administrador pode excluir este comentário.",
        ErrorType.Forbidden
    );

    public static readonly Error ReviewNotFound = new(
        "comment.review_not_found",
        "A avaliação informada não existe.",
        ErrorType.Validation
    );
}
