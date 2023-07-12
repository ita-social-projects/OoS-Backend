namespace OutOfSchool.AuthorizationServer.Config;

public class QuartzConfig
{
    public const string Name = "Quartz";

    public string ConnectionStringKey { get; set; }

    public QuartzCronScheduleConfig CronSchedules { get; set; }
}