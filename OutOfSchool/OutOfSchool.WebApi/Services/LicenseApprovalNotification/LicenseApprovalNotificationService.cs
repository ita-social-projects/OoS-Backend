using IdentityModel.Client;
using System.Drawing.Text;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using Microsoft.AspNetCore.Builder;

namespace OutOfSchool.WebApi.Services.LicenseApprovalNotification;

public class LicenseApprovalNotificationService : ILicenseApprovalNotificationService, INotificationReciever
{
    private readonly INotificationService notificationService;
    private readonly ILogger<LicenseApprovalNotificationService> logger;
    private readonly IEntityRepository<string, User> userRepository;

    public LicenseApprovalNotificationService(INotificationService notificationService,
                                              ILogger<LicenseApprovalNotificationService> logger,
                                              IEntityRepository<string, User> userRepository)
    {
        this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task Generate(CancellationToken cancellation = default)
    {
        logger.LogInformation("License approval notification generating was started");

        var additionalData = new Dictionary<string, string>
        {
            { "Status", "LicenseApproval" },
        };

        await notificationService
            .Create(NotificationType.System, NotificationAction.LicenseApproval, Guid.Empty, this, additionalData)
            .ConfigureAwait(false);

        logger.LogInformation("License approval notification generating was finished");
    }

    public async Task<IEnumerable<string>> GetNotificationsRecipientIds(NotificationAction action, Dictionary<string, string> additionalData, Guid objectId)
    {
        // TODO Add filter for AreaAdmin when he will be created, and delete filter for TechAdmin after that
        return (await userRepository
            .GetByFilter(u => u.Role.Equals(nameof(Role.TechAdmin), StringComparison.CurrentCultureIgnoreCase)
                      || u.Role.Equals(nameof(Role.RegionAdmin), StringComparison.CurrentCultureIgnoreCase))
            .ConfigureAwait(false))
            .Select(a => a.Id);
    }
}
