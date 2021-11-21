using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.WebApi.Common.Resources.References
{
    /// <summary>
    /// Base class for a resource instance.
    /// </summary>
    internal abstract class Resource
    {
        private readonly ResourceManager resourceManager;

        protected Resource(ResourceManager resourceManager)
        {
            this.resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
        }

        /// <summary>
        /// Gets essential string from resources by the given resourceKey.
        /// </summary>
        /// <param name="resourceKey">The key for search in resources</param>
        /// <param name="culture">Provides information about a specific culture.</param>
        /// <returns>The <see cref="String"/> value that is taken from resources. If value is not found, it returns null.</returns>
        protected string GetResourceString(string resourceKey, CultureInfo culture = null)
        {
            return resourceManager.GetString(resourceKey, culture);
        }
    }
}
