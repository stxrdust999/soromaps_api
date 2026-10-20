using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class ActivityErrors
{
    public static readonly Error TooFar = new(
        "visit.too_far",
        "Você precisa estar a menos de 200 metros do local para confirmar seu check-in.",
        ErrorType.Validation
    );

    public static readonly Error AlreadyCheckedInToday = new(
        "visit.already_checked_in_today",
        "Você já realizou um check-in neste local hoje. Volte amanhã para registrar uma nova visita!",
        ErrorType.Conflict
    );

    public static readonly Error PlaceNotFound = new(
        "activity.place_not_found",
        "O local informado não foi encontrado ou não está aprovado.",
        ErrorType.NotFound
    );
}
