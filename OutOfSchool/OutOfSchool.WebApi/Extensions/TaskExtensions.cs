using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Extensions;

public static class TaskExtensions
{
    public static async Task<IActionResult> ProtectAndMap<TResult>([NotNull] this Task<SearchResult<TResult>> operation, [NotNull] Func<SearchResult<TResult>, IActionResult> mapped)
    {
        try
        {
            var result = await operation;

            return mapped(result);
        }
        catch (Exception)
        {
            return new ObjectResult("An unexpected error occurred while processing your request. Please try again or contact the administrator.")
            {
                StatusCode = 500,
            };
        }
    }
}