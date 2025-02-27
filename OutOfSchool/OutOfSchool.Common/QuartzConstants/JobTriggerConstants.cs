namespace OutOfSchool.Common.QuartzConstants;

public static class JobTriggerConstants
{
    public const string ElasticSearchWorkshopSynchronization = "elasticsearchWorkshopSyncJobTrigger";
    public const string ElasticSearchCompetitiveEventSynchronization = "elasticsearchCompetitiveEventSyncJobTrigger";
    public const string GcpImagesSynchronization = "gcpImagesJobTrigger";
    public const string StatisticReportsMaking = "statisticReportsMakingJobTrigger";
    public const string NotificationsClearing = "notificationsClearingJobTrigger";
    public const string ApplicationStatusChanging = "applicationStatusChangingJobTrigger";
    public const string AverageRatingCalculating = "averageRatingCalculatingJobTrigger";
    public const string LicenseApprovalNotification = "licenseApprovalNotificationJobTrigger";
    public const string EmailSender = "emailSenderJobTrigger";
}
