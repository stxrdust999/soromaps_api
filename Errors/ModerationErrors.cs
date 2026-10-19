using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class ModerationErrors
{
    public static readonly Error PlaceNotFound = new(
        "moderation.place_not_found",
        "O local informado para moderação não foi encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error StatusConflict = new(
        "moderation.status_conflict",
        "O status do local já foi alterado por outro moderador.",
        ErrorType.Conflict
    );

    public static readonly Error ReasonRequired = new(
        "moderation.reason_required",
        "É obrigatório informar um motivo válido ao devolver ou rejeitar um local.",
        ErrorType.Validation
    );

    public static readonly Error InvalidDecision = new(
        "moderation.invalid_decision",
        "A decisão de moderação deve ser 'approved', 'returned' ou 'rejected'.",
        ErrorType.Validation
    );

    public static readonly Error NothingToUndo = new(
        "moderation.nothing_to_undo",
        "Não há decisão anterior registrada para este local.",
        ErrorType.NotFound
    );

    public static readonly Error UndoExpired = new(
        "moderation.undo_expired",
        "O prazo de 24 horas para desfazer esta decisão já expirou.",
        ErrorType.Conflict
    );
}
