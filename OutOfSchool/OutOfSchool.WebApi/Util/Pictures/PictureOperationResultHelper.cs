using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.Pictures;

namespace OutOfSchool.WebApi.Util.Pictures
{
    public static class PictureOperationResultHelper
    {
        public static IEnumerable<PictureOperationResult> GetFailedResults(this IEnumerable<PictureOperationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return results.Where(por => por.Succeeded == false);
        }
    }
}
