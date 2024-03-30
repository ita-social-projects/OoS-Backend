using System;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace OutOfSchool.EmailSender;

public class EmailSenderService : IEmailSenderService
{
    private readonly IScheduler scheduler;

    public EmailSenderService()
    {
        scheduler = new StdSchedulerFactory().GetScheduler().Result;
        scheduler.Start().Wait();
    }

    public async Task SendAsync(string email, string subject, (string html, string plain) content, DateTime? expirationTime = null)
    {
        expirationTime ??= DateTime.MaxValue;

        var jobData = new JobDataMap
        {
            { "Email", email },
            { "Subject", subject },
            { "HtmlContent", EncodeToBase64(content.html) },
            { "PlainContent", EncodeToBase64(content.plain) },
            { "ExpirationTime", expirationTime },
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