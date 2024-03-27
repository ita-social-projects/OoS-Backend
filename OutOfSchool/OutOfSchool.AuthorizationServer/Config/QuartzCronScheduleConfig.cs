namespace OutOfSchool.AuthorizationServer.Config;

public class QuartzCronScheduleConfig
{
    public const string Name = "CronSchedules";

    public string GcpImagesSyncCronScheduleString { get; set; }

    public string StatisticReportsMakingCronScheduleString { get; set; }
}