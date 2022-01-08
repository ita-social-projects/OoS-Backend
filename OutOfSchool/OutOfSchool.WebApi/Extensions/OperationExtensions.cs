using System;
using System.Globalization;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;

namespace OutOfSchool.WebApi.Extensions
{
    public static class OperationExtensions
    {
        public static OperationError GetOperationError(this ImagesOperationErrorCode code)
        {
            return CreateOperationError(code.ToString(), code.GetResourceValue());
        }

        public static OperationError CreateOperationError(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(@$"Code [{code}] cannot be null or empty.", nameof(code));
            }

            return new OperationError
            {
                Code = code,
                Description = description ?? string.Empty,
            };
        }
    }
}
