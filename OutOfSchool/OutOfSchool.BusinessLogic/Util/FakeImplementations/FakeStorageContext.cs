using OutOfSchool.ExternalFileStore;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

public class FakeStorageContext(FakeStorageClient client, string bucketName) : IStorageContext<FakeStorageClient>
{
    public FakeStorageClient StorageClient { get; } = client;

    public string BucketName { get; } = bucketName;
}
