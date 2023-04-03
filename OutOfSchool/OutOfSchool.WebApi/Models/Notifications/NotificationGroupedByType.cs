using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Notifications;

public class NotificationGroupedByType
{
    [Required]
    public NotificationType Type { get; set; }

    public int Amount { get; set; }

    public List<NotificationGrouped> GroupedByAdditionalData { get; set; }
}