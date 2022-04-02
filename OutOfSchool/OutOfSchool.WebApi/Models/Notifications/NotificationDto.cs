using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string Data { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationAction Action { get; set; }

        [Required]
        public DateTimeOffset CreatedDateTime { get; set; }

        public DateTimeOffset? ReadDateTime { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
