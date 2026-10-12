using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "auth.invalid_credentials",
        "E-mail ou senha inválidos.",
        ErrorType.Unauthorized
    );

    public static readonly Error InvalidRefreshToken = new(
        "auth.invalid_refresh_token",
        "Refresh token inválido, expirado ou revogado.",
        ErrorType.Unauthorized
    );
}
