namespace OutOfSchool.BusinessLogic.Services.LicenseApprovalNotification;

public interface ILicenseApprovalNotificationService
{
    Task Generate(CancellationToken cancellation = default);
}
