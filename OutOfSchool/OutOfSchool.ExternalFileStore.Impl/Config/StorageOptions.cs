namespace OutOfSchool.ExternalFileStore.Config;

/// <summary>
/// Contains configuration for file storage providers and containers.
/// </summary>
public class StorageOptions
{
    public const string SectionName = "FileStorage";
    
    /// <summary>
    /// Gets or sets the active storage provider to use.
    /// </summary>
    public StorageProviderType Provider { get; set; } = StorageProviderType.GoogleCloud;

    /// <summary>
    /// Gets or sets the configuration for storage containers.
    /// </summary>
    public ContainersConfig Containers { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for storage providers.
    /// </summary>
    public ProvidersConfig Providers { get; set; } = new();
}

/// <summary>
/// Contains configuration for different types of storage containers.
/// </summary>
public class ContainersConfig
{
    /// <summary>
    /// Gets or sets the configuration for image storage.
    /// </summary>
    public ContainerConfig Images { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration for file storage.
    /// </summary>
    public ContainerConfig Files { get; set; } = new();
}

/// <summary>
/// Contains configuration for a storage container.
/// </summary>
public class ContainerConfig
{
    /// <summary>
    /// Gets or sets a bucket name of storage.
    /// </summary>
    public string BucketName { get; set; }
}

/// <summary>
/// Contains configuration for different storage providers.
/// </summary>
public class ProvidersConfig
{
    /// <summary>
    /// Gets or sets the Google Cloud Storage configuration.
    /// </summary>
    public GoogleCloudConfig GoogleCloud { get; set; } = new();

    /// <summary>
    /// Gets or sets the Amazon S3 configuration.
    /// </summary>
    public AmazonS3Config AmazonS3 { get; set; } = new();

    /// <summary>
    /// Gets or sets the fake storage configuration for testing.
    /// </summary>
    public FakeConfig Fake { get; set; } = new();
}

/// <summary>
/// Contains a configuration that is essential for Google Cloud Storage.
/// </summary>
public class GoogleCloudConfig
{
    /// <summary>
    /// Gets or sets a file of Google credential.
    /// </summary>
    public string CredentialFilePath { get; set; }

    /// <summary>
    /// Gets or sets the Google Cloud project identifier.
    /// </summary>
    public string ProjectId { get; set; }
}

/// <summary>
/// Contains configuration for Amazon S3 storage.
/// </summary>
public class AmazonS3Config
{
    /// <summary>
    /// Gets or sets the AWS access key identifier.
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// Gets or sets the AWS secret access key.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the AWS region for the S3 bucket.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Gets or sets the S3 service URL endpoint.
    /// </summary>
    public string ServiceUrl { get; set; }
}

/// <summary>
/// Contains configuration for fake storage provider used in testing.
/// </summary>
public class FakeConfig
{
}