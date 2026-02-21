using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Util;

public static class OperationResultHelper
{
    public static IEnumerable<OperationResult> GetFailedResults(this IEnumerable<OperationResult> results)
    {
        if (results == null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        return results.Where(x => !x.Succeeded);
    }

    public static IDictionary<TKey, OperationResult> GetFailedResults<TKey>(this IDictionary<TKey, OperationResult> results)
    {
        if (results == null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        return results.Where(x => !x.Value.Succeeded).ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Returns a standardized "Not Found" result for an entity.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was not found.</typeparam>
    /// <param name="id">The unique identifier of the missing entity.</param>
    /// <returns>A failed <see cref="Result{T}"/> indicating that the entity was not found.</returns>
    public static Result<T> NotFoundResult<T>(Guid id)
    {
        var entityName = typeof(T).Name;

        return Result<T>.Failed(new OperationError
        {
            Code = "404",
            Description = $"{entityName} with Id = {id} was not found"
        });
    }
}