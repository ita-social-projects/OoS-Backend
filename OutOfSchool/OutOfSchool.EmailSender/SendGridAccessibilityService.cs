using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Quartz;

namespace OutOfSchool.EmailSender;

public interface ISendGridAccessibilityService 
{
    bool IsSendGridAccessible { get; set; }
}
public class SendGridAccessibilityService : ISendGridAccessibilityService
{
    public bool IsSendGridAccessible { get; set; }
}

public class EmailSenderJobListener : IJobListener
{
    private readonly ISendGridAccessibilityService sendGridAccessibilityService;

    public string Name => "Email Sender Job Listener";

    public EmailSenderJobListener(ISendGridAccessibilityService sendGridAccessibilityService)
    {
        this.sendGridAccessibilityService = sendGridAccessibilityService;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        if(jobException != null) 
        {
            sendGridAccessibilityService.IsSendGridAccessible = false;
        }
        return Task.CompletedTask;
    }
}

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

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!sendGridAccessibilityService.IsSendGridAccessible)
        {
            logger.LogInformation("SendGrid is inaccessible. Email Sender Job execution vetoed.");
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
