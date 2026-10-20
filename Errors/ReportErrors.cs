using soromaps_api.Results;

namespace soromaps_api.Errors;

public static class ReportErrors
{
    public static readonly Error CannotReportOwn = new(
        "report.cannot_report_own",
        "Você não pode denunciar seu próprio conteúdo.",
        ErrorType.Conflict
    );

    public static readonly Error AlreadyReported = new(
        "report.already_reported",
        "Você já enviou uma denúncia para este conteúdo.",
        ErrorType.Conflict
    );

    public static readonly Error TargetNotFound = new(
        "report.target_not_found",
        "O conteúdo informado para denúncia não foi encontrado.",
        ErrorType.NotFound
    );

    public static readonly Error InvalidTargetType = new(
        "report.invalid_target_type",
        "O tipo de conteúdo denunciado deve ser 'place', 'review' ou 'comment'.",
        ErrorType.Validation
    );

    public static readonly Error TargetHasNoOpenReports = new(
        "report.no_open_reports",
        "Não existem denúncias abertas para este conteúdo.",
        ErrorType.NotFound
    );
}
