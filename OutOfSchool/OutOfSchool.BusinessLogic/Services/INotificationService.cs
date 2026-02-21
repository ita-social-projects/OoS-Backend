using OutOfSchool.BusinessLogic.Models.Notifications;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Notification entity.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="notificationDto">Notificaton entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<NotificationDto> Create(NotificationDto notificationDto);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="type">Notificaton type to add.</param>
    /// <param name="action">Notificaton action to add.</param>
    /// <param name="objectId">ObjectId.</param>
    /// <param name="recipientsIds">Id's for sending notifications.</param>
    /// <param name="additionalData">Data need to save with notification.</param>
    /// <param name="groupedData">Field is used in grouping notifications for response type NotificationGrouped.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Create(
        NotificationType type,
        NotificationAction action,
        Guid objectId,
        IEnumerable<string> recipientsIds,
        Dictionary<string, string> additionalData = null,
        string groupedData = null);

    /// <summary>
    /// Delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the Notification in table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    /// Set property ReadDateTime in the notification.
    /// </summary>
    /// <param name="id">Key of the Notification in table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<NotificationDto> Read(Guid id);

    /// <summary>
    /// Set property ReadDateTime field in all unreaded notifications.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task ReadAll(string userId);

    /// <summary>
    /// Set property ReadDateTime in notifications with specified notificationType.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <param name="notificationType">NotificationType.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task ReadUsersNotificationsByType(string userId, NotificationType notificationType);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<NotificationDto> GetById(Guid id);

    /// <summary>
    /// Get all user's notification grouped by type from the database.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<NotificationGroupedAndSingle> GetAllUsersNotificationsGroupedAsync(string userId);

    /// <summary>
    /// Get all user's notification from the database.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <param name="notificationType">Type of notifications.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<NotificationDto>> GetAllUsersNotificationsByFilterAsync(string userId, NotificationType? notificationType);

    /// <summary>
    /// Get amount of new notifications for user.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<int> GetAmountOfNewUsersNotificationsAsync(string userId);
}