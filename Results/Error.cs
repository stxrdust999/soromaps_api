namespace soromaps_api.Results;

/**
 * Representa um erro de negócio imutável.
 * Combina um código de máquina para i18n/frontend, mensagem descritiva e categoria HTTP.
 */
public sealed record Error(string Code, string Message, ErrorType Type);
