﻿using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services.LicenseApprovalNotification;

public class LicenseApprovalNotificationService : ILicenseApprovalNotificationService
{
    private readonly INotificationService notificationService;
    private readonly ILogger<LicenseApprovalNotificationService> logger;
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;

    public LicenseApprovalNotificationService(
        INotificationService notificationService,
        ILogger<LicenseApprovalNotificationService> logger,
        IEntityRepositorySoftDeleted<string, User> userRepository)
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

        var recipientsIds = await GetNotificationsRecipientIds().ConfigureAwait(false);

        await notificationService
            .Create(NotificationType.System, NotificationAction.LicenseApproval, Guid.Empty, recipientsIds, additionalData)
            .ConfigureAwait(false);

        logger.LogInformation("License approval notification generating was finished");
    }

    private async Task<IEnumerable<string>> GetNotificationsRecipientIds()
    {
        // TODO Add filter for AreaAdmin when he will be created, and delete filter for TechAdmin after that
        return (await userRepository
            .GetByFilter(u => u.Role.Equals(nameof(Role.TechAdmin), StringComparison.CurrentCultureIgnoreCase)
                      || u.Role.Equals(nameof(Role.RegionAdmin), StringComparison.CurrentCultureIgnoreCase))
            .ConfigureAwait(false))
            .Select(a => a.Id);
    }
}
