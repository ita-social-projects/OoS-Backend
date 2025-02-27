using OutOfSchool.ExternalFileStore;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

public class FakeStorageContext(IFakeStorageClient client, string bucketName) : IStorageContext<IFakeStorageClient>
{
    public IFakeStorageClient StorageClient { get; } = client;

    public string BucketName { get; } = bucketName;
}
