using System;
using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Services;
using Quartz;

namespace OutOfSchool.EmailSender.Quartz;

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
        if (jobException != null)
        {
            sendGridAccessibilityService.SetSendGridInaccessible(DateTimeOffset.Now);
        }
        return Task.CompletedTask;
    }
}
