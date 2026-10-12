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
}
