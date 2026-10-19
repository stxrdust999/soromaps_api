using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class TagErrors
{
    public static readonly Error NotFound = new(
        "tag.not_found",
        "Tag não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error NameTaken = new(
        "tag.name_taken",
        "Já existe uma tag cadastrada com esse nome ou slug.",
        ErrorType.Conflict
    );
}
