using Quartz;

namespace OutOfSchool.WebApi.Services.LicenseApprovalNotification;

public class LicenseApprovalNotificationQuartzJob : IJob
{
    public readonly ILicenseApprovalNotificationService licenseApprovalNotificationService;
    public readonly ILogger<LicenseApprovalNotificationQuartzJob> logger;

    public LicenseApprovalNotificationQuartzJob(ILicenseApprovalNotificationService licenseApprovalNotificationService,
                                                ILogger<LicenseApprovalNotificationQuartzJob> logger)
    {
        this.licenseApprovalNotificationService = licenseApprovalNotificationService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("License approval notification Quartz job was started");

        await licenseApprovalNotificationService.Generate().ConfigureAwait(false);

        logger.LogInformation("License approval notification Quartz job was finished");
    }
}
