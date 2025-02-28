using OutOfSchool.ExternalFileStore;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

/// <summary>
/// Represents a storage context for fake File Storage.
/// </summary>
public class FakeStorageContext(IFakeStorageClient client, string bucketName) : IStorageContext<IFakeStorageClient>
{
    /// <summary>
    /// Gets the Fake Storage Client.
    /// </summary>
    public IFakeStorageClient StorageClient { get; } = client;

    /// <summary>
    /// Gets the fake name of the storage bucket.
    /// </summary>
    public string BucketName { get; } = bucketName;
}
