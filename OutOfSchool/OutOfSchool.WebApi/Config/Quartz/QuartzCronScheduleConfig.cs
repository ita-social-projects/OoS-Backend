namespace OutOfSchool.WebApi.Config.Quartz;

public class QuartzCronScheduleConfig
{
    public const string Name = "CronSchedules";

    public string GcpImagesSyncCronScheduleString { get; set; }

    public string StatisticReportsMakingCronScheduleString { get; set; }

    public string NotificationsClearingCronScheduleString { get; set; }

    public string ApplicationStatusChangingCronScheduleString { get; set; }

    public string AverageRatingCalculatingCronScheduleString { get; set; }
}
