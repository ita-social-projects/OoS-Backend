using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for getting notification recievers.
    /// All services, which use notifications, implement this interface.
    /// </summary>
    public interface INotificationReciever
    {
        /// <summary>
        /// Get user's ids for notification.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="additionalData">Additional data.</param>
        /// <param name="objectId">ObjectId.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<string>> GetNotificationsRecipients(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId);
    }
}
