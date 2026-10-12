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
        "Já existe uma conquista com esse nome.",
        ErrorType.Conflict
    );
}
