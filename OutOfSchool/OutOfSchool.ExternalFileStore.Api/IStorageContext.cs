namespace OutOfSchool.ExternalFileStore;

public interface IStorageContext<out TStorageClient>
{
    TStorageClient StorageClient { get; }

    string BucketName { get; }
}