using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "user.not_found",
        "Usuário não encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error EmailTaken = new(
        "user.email_taken",
        "Já existe uma conta cadastrada com esse e-mail.",
        ErrorType.Conflict
    );

    public static readonly Error LastAdmin = new(
        "user.last_admin",
        "Não é permitido remover ou rebaixar o único administrador do sistema.",
        ErrorType.Conflict
    );

    public static readonly Error InvalidCurrentPassword = new(
        "user.invalid_current_password",
        "A senha atual informada está incorreta.",
        ErrorType.Validation
    );

    public static readonly Error SamePassword = new(
        "user.same_as_current_password",
        "A nova senha não pode ser igual à senha atual.",
        ErrorType.Validation
    );
}
