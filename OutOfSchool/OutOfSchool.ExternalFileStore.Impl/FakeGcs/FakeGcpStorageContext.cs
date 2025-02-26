using Google.Cloud.Storage.V1;

namespace OutOfSchool.ExternalFileStore.FakeGcs;

public class FakeGcpStorageContext(StorageClient client, string bucketName) : IStorageContext<StorageClient>
{
    public StorageClient StorageClient { get; } = client;

    public string BucketName { get; } = bucketName;
}