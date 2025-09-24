namespace OutOfSchool.Common.QuartzConstants;

public static class JobConstants
{
    public const string ElasticSearchWorkshopSynchronization = "elasticsearchWorkshopSynchronizationJob";
    public const string ElasticSearchCompetitiveEventSynchronization = "elasticsearchCompetitiveEventSynchronizationJob";
    public const string GcpImagesSynchronization = "gcpImagesSynchronizationJob";
    public const string StatisticReportsMaking = "statisticReportsMakingJob";
    public const string NotificationsClearing = "notificationsClearingJob";
    public const string ApplicationStatusChanging = "applicationStatusChangingJob";
    public const string AverageRatingCalculating = "averageRatingCalculatingJob";
    public const string LicenseApprovalNotification = "licenseApprovalNotificationJob";
    public const string EmailSender = "emailSenderJob";
    public const string ThumbnailGeneration = "thumbnailGenerationJob";
}
