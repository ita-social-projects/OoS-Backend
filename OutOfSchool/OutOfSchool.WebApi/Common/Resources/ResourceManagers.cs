using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Resources
{
    /// <summary>
    /// Provides access to all resource managers.
    /// </summary>
    internal static class ResourceManagers
    {
        private static ResourceManager imageResourceManager;

        internal static ResourceManager ImageResourceManager => imageResourceManager ??= new ResourceManager(ResourceNames.Images, Assembly.GetExecutingAssembly());
    }
}
