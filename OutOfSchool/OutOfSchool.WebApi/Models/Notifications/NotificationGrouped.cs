using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Notifications;

public class NotificationGrouped
{
    [Required]
    public NotificationType Type { get; set; }

    public NotificationAction Action { get; set; }

    public string GroupedData { get; set; }

    public int Amount { get; set; }
}