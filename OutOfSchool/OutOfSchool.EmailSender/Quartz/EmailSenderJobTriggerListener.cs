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

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!sendGridAccessibilityService.IsSendGridAccessible(DateTimeOffset.Now))
        {
            logger.LogInformation("SendGrid is inaccessible. Email Sender Job execution vetoed.");
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
