using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.EmailSender.Quartz;
public class EmailSenderJobTriggerListener : ITriggerListener
{
    private readonly ISendGridAccessibilityService sendGridAccessibilityService;
    private readonly ILogger<EmailSenderJobTriggerListener> logger;

    public string Name => "Email Sender Job Trigger Listener";

    public EmailSenderJobTriggerListener(ISendGridAccessibilityService sendGridAccessibilityService, ILogger<EmailSenderJobTriggerListener> logger)
    {
        this.sendGridAccessibilityService = sendGridAccessibilityService;
        this.logger = logger;
    }

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!sendGridAccessibilityService.IsSendGridAccessible)
        {
            logger.LogInformation("SendGrid is inaccessible. Email Sender Job execution vetoed.");
            var currentTrigger = context.Trigger;

            var nextExecutionDate = (DateTimeOffset)sendGridAccessibilityService.GetNextStartDate();

            var updatedTrigger = currentTrigger.GetTriggerBuilder()
                .StartAt(nextExecutionDate)
                .Build();

            await context.Scheduler.RescheduleJob(currentTrigger.Key, updatedTrigger, cancellationToken);
            return true;
        }
        return false;
    }
}
