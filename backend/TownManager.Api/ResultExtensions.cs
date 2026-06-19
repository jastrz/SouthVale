using Serilog;
using TownManager.Application.Common;

namespace TownManager.Api;

public static class ResultExtensions
{
    public static IResult ToHttpResponse(this Result result)
    {
        if (result.Succeeded)
            return Results.Ok();

        Log.Information("Request failed ({StatusCode}): {Errors}", result.StatusCode, result.Errors);

        return Results.Problem(
            statusCode: result.StatusCode,
            title: GetTitleForStatusCode(result.StatusCode),
            detail: string.Join(", ", result.Errors));
    }

    public static IResult ToHttpResponse<T>(this Result<T> result)
    {
        if (result.Succeeded)
            return Results.Ok(result.Value);

        Log.Information("Request failed ({StatusCode}): {Errors}", result.StatusCode, result.Errors);

        return Results.Problem(
            statusCode: result.StatusCode,
            title: GetTitleForStatusCode(result.StatusCode),
            detail: string.Join(", ", result.Errors));
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        400 => "Bad request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not found",
        409 => "Conflict",
        _ => "Error"
    };
}
