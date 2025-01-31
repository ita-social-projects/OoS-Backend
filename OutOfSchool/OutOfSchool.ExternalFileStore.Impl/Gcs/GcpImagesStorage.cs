using Google.Cloud.Storage.V1;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.Gcs;

/// <summary>
/// Represents an image storage.
/// </summary>
public class GcpImagesStorage(IStorageContext<StorageClient> storageContext)
    : GcpFilesStorageBase<ImageFileModel>(storageContext), IObjectImageStorage;