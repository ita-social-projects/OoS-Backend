using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.Common.Extensions;

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            return true;
        }

        return !enumerable.Any();
    }
}
