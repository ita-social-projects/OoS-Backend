﻿using Microsoft.Extensions.Logging;
using OutOfSchool.BusinessLogic.Services.NotificationsClearing;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class NotificationsClearingQuartsJob : IJob
{
    private readonly INotificationsClearingService notificationsClearingService;
    private readonly ILogger<NotificationsClearingQuartsJob> logger;

    public NotificationsClearingQuartsJob(
        INotificationsClearingService notificationsClearingService,
        ILogger<NotificationsClearingQuartsJob> logger)
    {
        this.notificationsClearingService = notificationsClearingService;
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Clearing notifications job was started");

        await notificationsClearingService.ClearNotifications().ConfigureAwait(false);

        logger.LogInformation("Clearing notifications job was finished");
    }
}
