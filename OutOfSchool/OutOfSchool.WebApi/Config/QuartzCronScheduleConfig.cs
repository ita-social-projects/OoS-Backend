namespace OutOfSchool.WebApi.Config
{
    public class QuartzCronScheduleConfig
    {
        public const string Name = "Quartz:CronSchedules";

        public string GcpImagesSyncCronScheduleString { get; set; }
    }
}