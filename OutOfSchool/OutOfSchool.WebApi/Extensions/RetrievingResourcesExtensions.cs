using System.Globalization;
using System.Resources;
using Google.Protobuf.WellKnownTypes;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;
using Enum = System.Enum;

namespace OutOfSchool.WebApi.Extensions
{
    public static class RetrievingResourcesExtensions
    {
        public static string GetResourceValue(this ImagesOperationErrorCode code, CultureInfo culture = null)
        {
            var resourceKey = TryGetResourceKey(code);

            return GetStringFromResources(ResourceManagers.ImageResourceManager, resourceKey, culture);
        }

        public static string GetResourceKey(this ImagesOperationErrorCode code) => TryGetResourceKey(code);

        internal static string GetStringFromResources(ResourceManager resourceManager, string resourceKey, CultureInfo culture = null)
        {
            return string.IsNullOrEmpty(resourceKey) ? null :
                resourceManager.GetString(resourceKey, culture);
        }

        private static string TryGetResourceKey(Enum value)
        {
            var resourceKeyAttribute = value.GetAttribute<ResourcesKeyAttribute>();
            return resourceKeyAttribute?.ResourcesKey;
        }
    }
}
