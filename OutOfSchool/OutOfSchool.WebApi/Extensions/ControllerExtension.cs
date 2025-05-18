using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Extensions;

public static class ControllerExtension
{
    public static IActionResult SearchResultToOkOrNoContent<T>(this ControllerBase controller, SearchResult<T> searchResult)
    {
        ExtensionValidation(controller);

        // searchResult.TotalAmount < searchResult.Entities.Count - checking bug variant
        if (searchResult.IsNullOrEmpty() || searchResult.TotalAmount < searchResult.Entities?.Count)
        {
            return controller.NoContent();
        }

        return controller.Ok(searchResult);
    }

    public static string GetJwtClaimByName(this ControllerBase controller, string claimName)
    {
        ExtensionValidation(controller);
        return controller.User.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
    }

    public static void ValidateId(this ControllerBase controller, long id, IStringLocalizer<SharedResource> localizer)
    {
        if (id < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(id), localizer["The id cannot be less than 1."]);
        }
    }

    private static void ExtensionValidation(ControllerBase controller)
    {
        if (controller == null)
        {
            throw new ArgumentException(@"Controller cannot be null", nameof(controller));
        }
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> into an appropriate <see cref="IActionResult"/>.
    /// Automatically maps error codes to HTTP status responses.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the result.</typeparam>
    /// <param name="controller">The controller handling the request.</param>
    /// <param name="result">The result to convert.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result.</returns>
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.Succeeded)
        {
            return controller.Ok(result.Value);
        }

        var error = result.OperationResult.Errors.FirstOrDefault();
        if (error != null)
        {
            return error.Code switch
            {
                "400" => controller.BadRequest(error.Description),
                "403" => controller.Forbid(),
                "404" => controller.NotFound(error.Description),
                "409" => controller.Conflict(error.Description),
                _ => controller.StatusCode(500, error.Description ?? "Unexpected error")
            };
        }

        return controller.StatusCode(500, "Unexpected error");
    }
}