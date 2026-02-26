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
}
