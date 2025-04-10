namespace OutOfSchool.ExternalFileStore.Models;
/// <summary>
/// Represents data needed for monitoring request
/// </summary>
public class MonitoringRequest
{
    /// <summary>
    /// The bucket where the event occurred
    /// </summary>
    public string BucketName { get; set; }
    
    /// <summary>
    /// Object key prefix filter
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Object key suffix filter
    /// </summary>
    public string Suffix { get; set; } = string.Empty;
}
