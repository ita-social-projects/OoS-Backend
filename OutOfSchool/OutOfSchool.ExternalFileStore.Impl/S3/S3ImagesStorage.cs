using Minio;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.S3;

/// <summary>
/// Represents an image storage.
/// </summary>
public class S3ImagesStorage(IStorageContext<IMinioClient> storageContext)
    : S3FilesStorageBase<ImageFileModel>(storageContext), IObjectImageStorage
{
}