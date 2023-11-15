using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files;

/// <summary>
/// Represents an images storage.
/// </summary>
public class GcpImagesStorage : GcpFilesStorageBase<ImageFileModel>, IImageFilesStorage
{
    public GcpImagesStorage(IGcpStorageContext storageContext)
        : base(storageContext)
    {
    }
}