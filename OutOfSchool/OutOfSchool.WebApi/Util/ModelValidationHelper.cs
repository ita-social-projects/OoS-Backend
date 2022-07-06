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
        stringBuilder.AppendLine($"Validation of {nameof(OffsetFilter)} faild.");

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
}