namespace OutOfSchool.WebApi.Config.Quartz
{
    public class QuartzCronScheduleConfig
    {
        public const string Name = "CronSchedules";

        public string GcpImagesSyncCronScheduleString { get; set; }
    }
}