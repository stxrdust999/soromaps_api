using Microsoft.AspNetCore.Mvc;
using soromaps_api.Results;

namespace soromaps_api.Controllers;

/**
 * Controller base para padronização da camada HTTP.
 * Centraliza o mapeamento de erros de domínio retornados pelo Result pattern
 * para o formato de resposta RFC 7807 (ProblemDetails).
 */
public abstract class ApiControllerBase : ControllerBase
{
    /**
     * Converte um erro de domínio em uma resposta ProblemDetails com status HTTP correspondente.
     * Mapeia o ErrorType semantico para 400, 401, 403, 404, 409 ou 500, anexando o código de erro.
     */
    protected ActionResult HandleFailure(Error error)
    {
        /** Traduz a categoria de erro de domínio para o código de status HTTP correspondente. */
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        /** Produz o ProblemDetails contendo a mensagem em português e a extensão code para o front. */
        return Problem(
            statusCode: statusCode,
            detail: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code }
        );
    }
}
