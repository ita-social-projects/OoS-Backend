using Google.Cloud.Storage.V1;

namespace OutOfSchool.ExternalFileStore.Gcs;

public class GcpStorageContext(StorageClient client, string bucketName) : IStorageContext<StorageClient>
{
    public StorageClient StorageClient { get; } = client;

    public string BucketName { get; } = bucketName;
}