namespace soromaps_api.Results;

/**
 * Encapsula o resultado de uma operação que pode ser bem-sucedida ou falhar sem lançar exceções.
 * Fornece a base para o Result pattern adotado nos Services.
 */
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}
