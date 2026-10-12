namespace soromaps_api.Results;

/**
 * Categorias semânticas de falha de negócio.
 * Permitem que a camada HTTP determine o status code apropriado sem conhecer os detalhes da regra.
 */
public enum ErrorType
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}
