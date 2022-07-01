namespace OutOfSchool.WebApi.Config.Quartz
{
    public class QuartzConfig
    {
        public const string Name = "Quartz";

        public string ConnectionStringKey { get; set; }

        public QuartzCronScheduleConfig CronSchedules { get; set; }
    }
}