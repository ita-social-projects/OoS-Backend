namespace OutOfSchool.WebApi.Config
{
    public class ElasticsearchSynchronizationSchedulerConfig
    {
        public const string Name = "ElasticsearchSynchronizationScheduler";

        public int OperationsPerTask { get; set; }

        public int DelayBetweenTasksInMilliseconds { get; set; }
    }
}
