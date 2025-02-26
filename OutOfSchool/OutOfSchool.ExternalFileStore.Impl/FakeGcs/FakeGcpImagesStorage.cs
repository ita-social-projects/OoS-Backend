using Google.Cloud.Storage.V1;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeGcs;

/// <summary>
/// Represents a fake Gcp image storage.
/// </summary>
public class FakeGcpImagesStorage(IStorageContext<StorageClient> storageContext)
    : FakeGcpFilesStorageBase<ImageFileModel>(storageContext), IObjectImageStorage;
