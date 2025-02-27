namespace OutOfSchool.ExternalFileStore;

/// <summary>
/// Defines the available storage provider types.
/// </summary>
public enum StorageProviderType
{
    /// <summary>
    /// Google Cloud Storage provider
    /// </summary>
    GoogleCloud,

    /// <summary>
    /// Amazon S3 Storage provider
    /// </summary>
    AmazonS3,

    /// <summary>
    /// Fake storage provider for testing
    /// </summary>
    Fake
} 