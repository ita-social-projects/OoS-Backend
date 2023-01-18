namespace OutOfSchool.WebApi.Common.QuartzConstants;

public static class JobTriggerConstants
{
    public const string ElasticSearchSynchronization = "elasticsearchJobTrigger";
    public const string GcpImagesSynchronization = "gcpImagesJobTrigger";
    public const string StatisticReportsMaking = "statisticReportsMakingJobTrigger";
    public const string ApplicationStatusChanging = "applicationStatusChangingJobTrigger";
}