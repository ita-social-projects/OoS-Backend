﻿namespace OutOfSchool.BusinessLogic.Config;

public class ElasticsearchSynchronizationSchedulerConfig
{
    public static string SectionName { get; } = ElasticConfig.Name + ConfigurationPath.KeyDelimiter + "SynchronizationScheduler";

    public int OperationsPerTask { get; set; }

    public int DelayBetweenTasksInMilliseconds { get; set; }
}