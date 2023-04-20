namespace OutOfSchool.WebApi.Services.NotificationsClearing;

public interface INotificationsClearingService
{
    /// <summary>
    /// Asynchronously clearing old notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task ClearNotifications(CancellationToken cancellationToken = default);
}
