using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeImplementations;

/// <summary>
/// Represents a fake image storage.
/// </summary>
public class FakeImagesStorage(IStorageContext<IFakeStorageClient> storageContext)
    : FakeFilesStorageBase<ImageFileModel>(storageContext), IObjectImageStorage;