using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class FeedMuteErrors
{
    public static readonly Error AlreadyMuted = new(
        "feed_mute.already_muted",
        "Você já silenciou este escopo do feed.",
        ErrorType.Conflict
    );

    public static readonly Error NotFound = new(
        "feed_mute.not_found",
        "Filtro de silenciamento não encontrado.",
        ErrorType.NotFound
    );
}
