namespace OutOfSchool.BusinessLogic.Config;

public class NotificationsConfig
{
    public const string Name = "Notifications";

    public bool Enabled { get; set; }

    public List<string> Grouped { get; set; }
}