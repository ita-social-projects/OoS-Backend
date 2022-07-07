using System.Collections.Generic;

namespace OutOfSchool.WebApi.Models.Notifications;

public class NotificationGroupedAndSingle
{
    public IEnumerable<NotificationGrouped> NotificationsGrouped { get; set; }

    public IEnumerable<NotificationDto> Notifications { get; set; }
}