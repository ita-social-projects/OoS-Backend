namespace OutOfSchool.WebApi.Models.Notifications;

public class NotificationGroupedAndSingle
{
    public IEnumerable<NotificationGroupedByType> NotificationsGroupedByType { get; set; }

    public IEnumerable<NotificationDto> Notifications { get; set; }
}