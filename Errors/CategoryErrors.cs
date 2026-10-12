using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class CategoryErrors
{
    public static readonly Error NotFound = new(
        "category.not_found",
        "Categoria não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error NameTaken = new(
        "category.name_taken",
        "Já existe uma categoria cadastrada com esse nome.",
        ErrorType.Conflict
    );

    public static readonly Error InUse = new(
        "category.in_use",
        "Não é possível remover uma categoria que possui lugares vinculados.",
        ErrorType.Conflict
    );
}
