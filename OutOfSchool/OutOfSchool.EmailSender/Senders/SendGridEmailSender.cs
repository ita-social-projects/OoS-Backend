using System;
using System.Net;
using System.Threading.Tasks;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

public class SendGridEmailSender(ISendGridClient sendGridClient) : IEmailSender
{
    public async Task SendAsync(SendGridMessage sendGridMessage)
    {
        var response = await sendGridClient.SendEmailAsync(sendGridMessage).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new JobExecutionException("Email sending rate limit exceeded.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var sendError = await response.Body.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"Email was not sent with the following error: {sendError}");
        }
    }
}