namespace OutOfSchool.WebApi.Services.LicenseApprovalNotification;

public interface ILicenseApprovalNotificationService
{
    Task Generate(CancellationToken cancellation = default);
}
