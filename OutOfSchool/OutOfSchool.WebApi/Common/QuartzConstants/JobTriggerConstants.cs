namespace OutOfSchool.WebApi.Common.QuartzConstants;

public static class JobTriggerConstants
{
    public const string ElasticSearchSynchronization = "elasticsearchJobTrigger";
    public const string GcpImagesSynchronization = "gcpImagesJobTrigger";
    public const string StatisticReportsMaking = "statisticReportsMakingJobTrigger";
    public const string NotificationsClearing = "notificationsClearingJobTrigger";
    public const string ApplicationStatusChanging = "applicationStatusChangingJobTrigger";
    public const string AverageRatingCalculating = "averageRatingCalculatingJobTrigger";
    public const string LicenseApprovalNotification = "licenseApprovalNotificationJobTrigger";
}
