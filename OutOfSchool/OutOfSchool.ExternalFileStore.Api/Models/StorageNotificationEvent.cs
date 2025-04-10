namespace OutOfSchool.ExternalFileStore.Models;
/// <summary>
/// Represents a storage notification event
/// </summary>
public class StorageNotificationEvent
{
    /// <summary>
    /// The bucket where the event occurred
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// The object key that triggered the event
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the event occurred
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.UtcNow;    
}
