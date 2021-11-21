using System;
using System.Collections.Generic;
using System.Linq;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Util.Images
{
    public static class OperationResultHelper
    {
        public static IEnumerable<OperationResult> GetFailedResults(this IEnumerable<OperationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return results.Where(por => por.Succeeded == false);
        }

        public static IDictionary<TKey, OperationResult> GetFailedResults<TKey>(this IDictionary<TKey, OperationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return results.Where(por => por.Value.Succeeded == false).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
