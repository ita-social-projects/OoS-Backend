using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

/// <summary>
/// Represents a fake image storage.
/// </summary>
public class FakeImagesStorage(IStorageContext<FakeStorageClient> storageContext)
    : FakeFilesStorageBase<ImageFileModel>(storageContext), IObjectImageStorage;