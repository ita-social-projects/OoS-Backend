using System.Resources;

namespace OutOfSchool.WebApi.Common.Resources;

/// <summary>
/// Provides access to all resource managers.
/// </summary>
internal static class ResourceManagers
{
    internal static ResourceManager ImageResourceManager => WebApi.Resources.Images.ImageResource.ResourceManager;
}