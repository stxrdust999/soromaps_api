namespace soromaps_api.Results;

/**
 * Especialização tipada de Result que transporta um valor em caso de sucesso.
 * Impede o acesso acidental ao valor quando a operação resultou em falha.
 */
public sealed class Result<TValue> : Result
{
    private readonly TValue? value;

    private Result(TValue value) : base(true, null) => this.value = value;
    private Result(Error error) : base(false, error) { }

    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("Não é possível acessar o valor de um resultado com falha.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static new Result<TValue> Failure(Error error) => new(error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
