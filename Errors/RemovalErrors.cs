using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class RemovalErrors
{
    public static readonly Error TargetNotFound = new(
        "removal.target_not_found",
        "O conteúdo informado para remoção não foi encontrado ou já foi removido.",
        ErrorType.NotFound
    );

    public static readonly Error InvalidTargetType = new(
        "removal.invalid_target_type",
        "O tipo de alvo para remoção deve ser 'review' ou 'comment'.",
        ErrorType.Validation
    );

    public static readonly Error ReasonRequired = new(
        "removal.reason_required",
        "É obrigatório informar uma justificativa válida para a remoção.",
        ErrorType.Validation
    );
}
