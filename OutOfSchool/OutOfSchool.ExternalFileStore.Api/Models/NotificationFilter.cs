namespace OutOfSchool.ExternalFileStore.Models;
/// <summary>
/// Filter criteria for storage notifications
/// </summary>
public class NotificationFilter
{
    /// <summary>
    /// Object key prefix filter
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Object key suffix filter
    /// </summary>
    public string Suffix { get; set; } = string.Empty;
}
