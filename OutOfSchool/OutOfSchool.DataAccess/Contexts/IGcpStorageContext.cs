using Google.Cloud.Storage.V1;

namespace OutOfSchool.Services.Contexts;

public interface IGcpStorageContext
{
    StorageClient StorageClient { get; }

    string BucketName { get; }
}