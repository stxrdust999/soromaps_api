using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class AchievementErrors
{
    public static readonly Error NotFound = new(
        "achievement.not_found",
        "Conquista não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error NameTaken = new(
        "achievement.name_taken",
        "Já existe uma conquista cadastrada com esse nome.",
        ErrorType.Conflict
    );

    public static readonly Error TargetValueRequired = new(
        "achievement.target_value_required",
        "Quando o tipo de alvo for 'category' ou 'neighborhood', é obrigatório informar o valor correspondente.",
        ErrorType.Validation
    );
}
