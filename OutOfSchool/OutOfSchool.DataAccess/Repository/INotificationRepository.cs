using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface INotificationRepository : ISensitiveEntityRepository<Notification>
{
    /// <summary>
    /// Delete old notifications.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task ClearNotifications();

    /// <summary>
    /// Set ReadDateTime for the passed type.
    /// </summary>
    /// <param name="userId">User's id for notifications.</param>
    /// <param name="notificationType">NotificationType.</param>
    /// <param name="dateTime">DateTime.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<Notification>> SetReadDateTimeByType(string userId, NotificationType notificationType, DateTimeOffset dateTime);
}