namespace OutOfSchool.WebApi.Common.QuartzConstants;

public static class JobConstants
{
    public const string ElasticSearchSynchronization = "elasticsearchSynchronizationJob";
    public const string GcpImagesSynchronization = "gcpImagesSynchronizationJob";
    public const string StatisticReportsMaking = "statisticReportsMakingJob";
    public const string ApplicationStatusChanging = "applicationStatusChangingJob";
}