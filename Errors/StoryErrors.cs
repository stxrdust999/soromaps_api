using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class StoryErrors
{
    public static readonly Error NotFound = new(
        "story.not_found",
        "Pauta editorial não encontrada.",
        ErrorType.NotFound
    );

    public static readonly Error SlugTaken = new(
        "story.slug_taken",
        "Já existe uma pauta publicada ou em rascunho com este slug.",
        ErrorType.Conflict
    );

    public static readonly Error UnapprovedPlacesLinked = new(
        "story.unapproved_places",
        "Todos os locais vinculados à pauta precisam estar aprovados para que ela possa ser publicada.",
        ErrorType.Validation
    );

    public static readonly Error NoPlacesLinked = new(
        "story.no_places_linked",
        "A pauta precisa conter pelo menos um local vinculado para ser publicada.",
        ErrorType.Validation
    );

    public static readonly Error AlreadyPublished = new(
        "story.already_published",
        "Esta pauta já se encontra publicada.",
        ErrorType.Conflict
    );
}
