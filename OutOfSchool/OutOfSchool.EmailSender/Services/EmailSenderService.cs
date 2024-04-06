using System;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.EmailSender.Quartz;
using Quartz;

namespace OutOfSchool.EmailSender.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly IScheduler scheduler;

    public EmailSenderService(IScheduler scheduler)
    {
        this.scheduler = scheduler;
        this.scheduler.Start().Wait();
    }

    public async Task SendAsync(string email, string subject, (string html, string plain) content, DateTime? expirationTime = null)
    {
        expirationTime ??= DateTime.MaxValue;

        var jobData = new JobDataMap
        {
            { EmailSenderStringConstants.Email, email },
            { EmailSenderStringConstants.Subject, subject },
            { EmailSenderStringConstants.HtmlContent, EncodeToBase64(content.html) },
            { EmailSenderStringConstants.PlainContent, EncodeToBase64(content.plain) },
            { EmailSenderStringConstants.ExpirationTime, expirationTime },
        };

        var job = JobBuilder.Create<EmailSenderJob>()
            .UsingJobData(jobData)
            .StoreDurably()
            .Build();

        await scheduler.AddJob(job, false);
    }

    private string EncodeToBase64(string toEncode)
    {
        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);

        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}