using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.EmailSender.Quartz;

public class EmailSenderJobListener : IJobListener
{
    private readonly ISendGridAccessibilityService sendGridAccessibilityService;
    private readonly ISchedulerFactory schedulerFactory;
    private readonly ILogger<EmailSenderJobListener> logger;

    public string Name => "Email Sender Job Listener";

    public EmailSenderJobListener(
        ISendGridAccessibilityService sendGridAccessibilityService,
        ISchedulerFactory schedulerFactory,
        ILogger<EmailSenderJobListener> logger)
    {
        this.sendGridAccessibilityService = sendGridAccessibilityService;
        this.schedulerFactory = schedulerFactory;
        this.logger = logger;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        if (jobException != null)
        {
            sendGridAccessibilityService.SetSendGridInaccessible(DateTimeOffset.Now);

            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            var newTrigger = TriggerBuilder
                .Create()
                .StartAt(sendGridAccessibilityService.GetAccessibilityTime(DateTimeOffset.Now))
                .Build();

            await scheduler.RescheduleJob(context.Trigger.Key, newTrigger, cancellationToken);
            logger.LogInformation("An error occured while sending email. Setting SendGrid to inaccesible.");
        }
        return;
    }
}
