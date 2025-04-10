namespace OutOfSchool.ExternalFileStore.NotificationInterfaces;
/// <summary>
/// Defines methods to start and stop listening process for storage notifications from MinIO
/// </summary>
public interface IStorageNotificationService
{
    // <summary>
    /// Starts listening for incoming storage notifications from MinIO.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StartListening();

    /// <summary>
    /// Stops listening for storage notifications from MinIO.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StopListening();
}
