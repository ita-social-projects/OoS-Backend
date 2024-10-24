using System.Resources;

namespace OutOfSchool.BusinessLogic.Common.Resources;

/// <summary>
/// Provides access to all resource managers.
/// </summary>
internal static class ResourceManagers
{
    internal static ResourceManager ImageResourceManager => BusinessLogic.Resources.Images.ImageResource.ResourceManager;
}