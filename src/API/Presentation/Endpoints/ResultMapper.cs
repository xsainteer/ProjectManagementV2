using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Presentation.Endpoints;

public static class ResultMapper
{
    public static IResult ToActionResult(Result result, int successStatusCode = StatusCodes.Status200OK)
    {
        return result.Match(
            () => Results.StatusCode(successStatusCode),
            MapError
        );
    }

    public static IResult ToActionResult<T>(Result<T> result, int successStatusCode = StatusCodes.Status200OK)
    {
        return result.Match(
            value => Results.Json(value, statusCode: successStatusCode),
            MapError
        );
    }

    private static IResult MapError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => Results.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
            ErrorType.Validation => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest),
            ErrorType.Conflict => Results.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
            ErrorType.Unauthorized => Results.Problem(error.Message, statusCode: StatusCodes.Status401Unauthorized),
            ErrorType.Unexpected => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest)
        };
    }
}