using System;
using System.Text;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Util;

public static class ModelValidationHelper
{
    public static void ValidateOffsetFilter(OffsetFilter offsetFilter)
    {
        if (offsetFilter == null)
        {
            throw new ArgumentNullException(nameof(offsetFilter));
        }

        var isValid = true;

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Validation of {nameof(OffsetFilter)} failed.");

        if (offsetFilter.Size < 0)
        {
            isValid = false;
            stringBuilder.AppendLine($"{nameof(OffsetFilter.Size)}: {offsetFilter.Size} cannot be negative.");
        }

        if (offsetFilter.From < 0)
        {
            isValid = false;
            stringBuilder.AppendLine($"{nameof(OffsetFilter.From)}: {offsetFilter.From} cannot be negative.");
        }

        if (!isValid)
        {
            throw new ArgumentException(stringBuilder.ToString(), nameof(offsetFilter));
        }
    }

    public static void ValidateExcludedIdFilter(ExcludeIdFilter filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        ValidateOffsetFilter(filter);

        var isValid = true;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Validation of {nameof(ExcludeIdFilter)} failed.");

        if (filter.ExcludedId != null && filter.ExcludedId == Guid.Empty)
        {
            isValid = false;
            stringBuilder.AppendLine($"{nameof(ExcludeIdFilter.ExcludedId)}: ExcludedId cannot be Empty Guid.");
        }

        if (!isValid)
        {
            throw new ArgumentException(stringBuilder.ToString(), nameof(filter));
        }
    }
}