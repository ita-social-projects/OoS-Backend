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
            var resourceKey = code.GetResourceKey();

            _ = resourceKey ?? throw new InvalidOperationException(
                    $"Unreal to get the resource key from {code} in {nameof(ImagesOperationErrorCode)}.");

            return CreateOperationError(code.ToString(), RetrievingResourcesExtensions.GetStringFromResources(ResourceManagers.ImageResourceManager, resourceKey));
        }

        public static OperationError CreateOperationError(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            return new OperationError
            {
                Code = code,
                Description = description ?? string.Empty,
            };
        }
    }
}
