using Minio;

namespace OutOfSchool.ExternalFileStore.S3;

public class S3StorageContext(IMinioClient storageClient, string bucketName) : IStorageContext<IMinioClient>
{
    public IMinioClient StorageClient { get; } = storageClient;
    public string BucketName { get; } = bucketName;
}