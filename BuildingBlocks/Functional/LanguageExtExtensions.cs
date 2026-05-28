using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApi.Template.BuildingBlocks.Functional;

public static class LanguageExtExtensions
{
    public static IResult ToHttpResult<T>(this Option<T> option) =>
        option.Match(
            Some: value => Results.Ok(value),
            None: () => Results.NotFound());

    public static IResult ToHttpResult<T>(this Fin<T> fin) =>
        fin.Match(
            Succ: value => Results.Ok(value),
            Fail: MapError);

    public static IResult ToCreatedResult<T>(this Fin<T> fin, Func<T, string> uriFactory) =>
        fin.Match(
            Succ: value => Results.Created(uriFactory(value), value),
            Fail: MapError);

    private static IResult MapError(Error error) =>
        error.Code switch
        {
            StatusCodes.Status400BadRequest => Results.BadRequest(ToProblemDetails(error)),
            StatusCodes.Status404NotFound => Results.NotFound(ToProblemDetails(error)),
            StatusCodes.Status409Conflict => Results.Conflict(ToProblemDetails(error)),
            StatusCodes.Status422UnprocessableEntity => Results.UnprocessableEntity(ToProblemDetails(error)),
            _ => Results.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError)
        };

    private static ProblemDetails ToProblemDetails(Error error) => new()
    {
        Status = error.Code,
        Title = error.Message,
        Type = $"https://httpstatuses.io/{error.Code}"
    };
}
