using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Common.Resources.References;

namespace OutOfSchool.WebApi.Common.Resources
{
    /// <summary>
    /// Provides access to all resource instances.
    /// </summary>
    internal static class Resources
    {
        internal static ImageResource ImageResource => ImageResource.Instance;
    }
}
