using System.Collections.Generic;

namespace OutOfSchool.WebApi.Config
{
    public class NotificationsConfig
    {
        public const string Name = "Notifications";

        public List<string> Grouped { get; set; }
    }
}