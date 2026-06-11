using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return ToProblem(result, controller);
    }

    public static IActionResult ToNoContentOrProblem(this Result result, ControllerBase controller)
    {
        return result.IsSuccess ? controller.NoContent() : ToProblem(result, controller);
    }

    public static IActionResult ToProblem(this Result result, ControllerBase controller)
    {
        var (status, title) = result.Status switch
        {
            ResultStatus.ValidationError => (StatusCodes.Status400BadRequest, "Bad Request"),
            ResultStatus.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ResultStatus.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            ResultStatus.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ResultStatus.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var detail = result.Error?.Message ?? "The request could not be completed.";
        var problem = ProblemDetailsFactory.Create(controller.HttpContext, status, title, detail);
        if (result.Error is not null)
        {
            problem.Extensions["code"] = result.Error.Code;
        }

        return controller.StatusCode(status, problem);
    }
}
