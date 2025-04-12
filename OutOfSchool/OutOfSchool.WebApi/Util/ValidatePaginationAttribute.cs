using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.WebApi.Util;

[AttributeUsage(AttributeTargets.Method)]
public class ValidatePaginationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var args = context.ActionArguments;

        if (args.TryGetValue("page", out var pageObj) &&
            pageObj is int page && page < 1)
        {
            context.Result = new BadRequestObjectResult("Page number must be greater than 0.");
            return;
        }

        if (args.TryGetValue("pageSize", out var pageSizeObj) &&
            pageSizeObj is int pageSize && pageSize < 1)
        {
            context.Result = new BadRequestObjectResult("Page size must be greater than 0.");
        }
    }
}

