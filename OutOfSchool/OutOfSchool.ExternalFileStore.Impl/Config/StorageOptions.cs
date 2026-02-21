using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.ExternalFileStore.Config;

/// <summary>
/// Contains configuration for file storage providers and containers.
/// </summary>
public class StorageOptions : IValidatableObject
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
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validate bucket names
        if (string.IsNullOrEmpty(Containers.Images?.BucketName))
        {
            yield return new ValidationResult(
                "Bucket name is required for Images container",
                [nameof(Containers.Images.BucketName)]);
        }

        // TODO: Add file bucket validation when we will use it.

        // Validate provider-specific settings
        var results = Provider switch
        {
            StorageProviderType.GoogleCloud => ValidateGoogleCloud(),
            StorageProviderType.AmazonS3 => ValidateAmazonS3(),
            _ => []
        };

        foreach (var result in results)
        {
            yield return result;
        }
    }

    private IEnumerable<ValidationResult> ValidateGoogleCloud()
    {
        // Placeholder for consistency, currently no option in GCP needs to be present.
        return [];
    }

    private IEnumerable<ValidationResult> ValidateAmazonS3()
    {
        if (string.IsNullOrEmpty(Providers?.AmazonS3?.AccessKey))
        {
            yield return new ValidationResult(
                "Access key is required for Amazon S3",
                [nameof(Providers.AmazonS3.AccessKey)]);
        }

        if (string.IsNullOrEmpty(Providers?.AmazonS3?.SecretKey))
        {
            yield return new ValidationResult(
                "Secret key is required for Amazon S3",
                [nameof(Providers.AmazonS3.SecretKey)]);
        }

        if (string.IsNullOrEmpty(Providers?.AmazonS3?.Region))
        {
            yield return new ValidationResult(
                "Region is required for Amazon S3",
                [nameof(Providers.AmazonS3.Region)]);
        }

        if (string.IsNullOrEmpty(Providers?.AmazonS3?.ServiceUrl))
        {
            yield return new ValidationResult(
                "Service URL is required for Amazon S3",
                [nameof(Providers.AmazonS3.ServiceUrl)]);
        }
    }
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