using Minio.DataModel.Notification;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.NotificationInterfaces;
/// <summary>
/// Represents notification handling method
/// </summary>
public interface IProcessNotificationService
{
    /// <summary>
    /// The method to handle a notification when received from Redis
    /// </summary>
    /// <param name="message"></param>
    void ProcessNotification(string message);

    /// <summary>
    /// Parses a received notification directly from MinIO
    /// </summary>
    /// <param name="notification">The raw notification received from MinIO.</param>
    /// <returns>The parsed <see cref="StorageNotificationEvent"/>.</returns>
    StorageNotificationEvent ParseMinioNotification(MinioNotificationRaw notification);
}
