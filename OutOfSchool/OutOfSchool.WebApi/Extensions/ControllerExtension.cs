using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
}