using System;
using System.Globalization;
using System.Resources;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;
using Enum = System.Enum;

namespace OutOfSchool.WebApi.Extensions
{
    /// <summary>
    /// Extensions for receiving essential data from app resources.
    /// </summary>
    public static class RetrievingResourcesExtensions
    {
        /// <summary>
        /// Returns the value of the string resource localized for the specified culture by the <see cref="ImagesOperationErrorCode"/> code.
        /// </summary>
        /// <param name="code">The <see cref="ImagesOperationErrorCode"/> code.</param>
        /// <param name="culture">The culture you wanna try to get resource value.</param>
        /// <returns>The value of the resource localized for the specified culture, or <c>null</c> if a resource key cannot be found in a resource set.</returns>
        public static string GetResourceValue(this ImagesOperationErrorCode code, CultureInfo culture = null)
        {
            var resourceKey = TryGetResourceKey(code);

            return GetStringFromResources(ResourceManagers.ImageResourceManager, resourceKey, culture);
        }

        /// <summary>
        /// Returns the value of the resource key by the <see cref="ImagesOperationErrorCode"/> code.
        /// </summary>
        /// <param name="code">The <see cref="ImagesOperationErrorCode"/> code.</param>
        /// <returns>The value of the resource key, or <c>null</c> if a resource key cannot be found in an <see cref="ImagesOperationErrorCode"/> set.</returns>
        public static string GetResourceKey(this ImagesOperationErrorCode code) => TryGetResourceKey(code);

        /// <summary>
        /// Returns the value of the string resource localized for the specified culture.
        /// </summary>
        /// <param name="resourceManager">The resource manager.</param>
        /// <param name="resourceKey">The key to find resource value.</param>
        /// <param name="culture">The culture you wanna try to get resource value.</param>
        /// <returns>The value of the resource localized for the specified culture, or <c>null</c> if a resource key cannot be found in a resource set.</returns>
        /// <exception cref="ArgumentNullException">When resourceManager is null.</exception>
        internal static string GetStringFromResources(ResourceManager resourceManager, string resourceKey, CultureInfo culture = null)
        {
            _ = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));

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
