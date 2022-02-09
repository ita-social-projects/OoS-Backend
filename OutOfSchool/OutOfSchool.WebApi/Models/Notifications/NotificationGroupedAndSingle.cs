using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models.Notifications
{
    public class NotificationGroupedAndSingle
    {
        public IEnumerable<NotificationGrouped> NotificationsGrouped { get; set; }

        public IEnumerable<NotificationDto> Notifications { get; set; }
    }
}
