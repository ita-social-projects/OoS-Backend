using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.NotificationInterfaces;
/// <summary>
/// Defines methods for subscribing to and unsubscribing from MinIO storage notifications for specific buckets and filters
/// </summary>
public interface IStorageNotificationHandler
{
    /// <summary>
    /// Subscribes to storage notifications for a specified bucket using the provided filter.
    /// </summary>
    /// <param name="bucket">The name of the bucket to subscribe to.</param>
    /// <param name="filter">The notification filter to apply (e.g., event types, prefix/suffix).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Subscribe(string bucket, NotificationFilter filter);

    /// <summary>
    /// Unsubscribes from storage notifications for a specified bucket using the provided filter.
    /// </summary>
    /// <param name="bucket">The name of the bucket to unsubscribe from.</param>
    /// <param name="filter">The notification filter used during subscription.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Unsubscribe(string bucket, NotificationFilter filter);
}
