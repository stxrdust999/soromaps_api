using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new(
        "comment.not_found",
        "Comentário não encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error ReviewNotFound = new(
        "comment.review_not_found",
        "A avaliação informada não foi encontrada.",
        ErrorType.Validation
    );

    public static readonly Error NotOwner = new(
        "comment.not_owner",
        "Apenas o autor ou um administrador pode alterar ou excluir este comentário.",
        ErrorType.Forbidden
    );

    public static readonly Error MaxDepthExceeded = new(
        "comment.max_depth_exceeded",
        "Respostas são limitadas a 1 nível de profundidade.",
        ErrorType.Validation
    );

    public static readonly Error ParentCommentNotFound = new(
        "comment.parent_not_found",
        "O comentário ao qual você está respondendo não existe.",
        ErrorType.NotFound
    );
}
