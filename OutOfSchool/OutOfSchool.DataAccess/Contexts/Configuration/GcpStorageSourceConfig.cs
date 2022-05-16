namespace OutOfSchool.Services.Contexts.Configuration
{
    /// <summary>
    /// Contains a configuration that is essential for Google Cloud Storage.
    /// </summary>
    public abstract class GcpStorageSourceConfig
    {
        /// <summary>
        /// Gets or sets a file of Google credential.
        /// </summary>
        public string CredentialFile { get; set; } // Should be set in environment variables

        /// <summary>
        /// Gets or sets a bucket name of Google Storage.
        /// </summary>
        public string BucketName { get; set; }
    }
}