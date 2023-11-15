using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Util;

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
}