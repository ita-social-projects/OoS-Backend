using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender;

[DisallowConcurrentExecution]
public class EmailSenderJob : IJob
{
    private readonly IOptions<EmailOptions> emailOptions;
    private readonly ILogger<EmailSenderJob> logger;
    private readonly ISendGridClient sendGridClient;

    public EmailSenderJob(
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailSenderJob> logger,
        ISendGridClient sendGridClient)
    {
        this.emailOptions = emailOptions;
        this.logger = logger;
        this.sendGridClient = sendGridClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("The email sender Quartz job started.");

        if (!emailOptions.Value.Enabled)
        {
            logger.LogError("The email sender is disabled.");
            return;
        }

        JobDataMap dataMap = context.JobDetail.JobDataMap;

        var email = dataMap.GetString("Email");
        var subject = dataMap.GetString("Subject");
        var htmlContent = DecodeFrom64(dataMap.GetString("HtmlContent"));
        var plainContent = DecodeFrom64(dataMap.GetString("PlainContent"));
        var expirationTime = dataMap.GetDateTime("ExpirationTime");

        if (expirationTime > DateTime.UtcNow)
        {
            logger.LogError("Email was not sent because expiration time passed: {Email}, {ExpirationTime}", email, expirationTime);
            return;
        }

        var message = new SendGridMessage()
        {
            From = new EmailAddress()
            {
                Email = emailOptions.Value.AddressFrom,
                Name = emailOptions.Value.NameFrom,
            },
            Subject = subject,
            HtmlContent = DecodeFrom64(htmlContent),
            PlainTextContent = DecodeFrom64(plainContent),
        };

        message.AddTo(new EmailAddress(email));

        var response = await sendGridClient.SendEmailAsync(message).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            logger.LogError("Email sending rate limit exceeded.");
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Email was not sent with the following error: {Error}", await response.Body.ReadAsStringAsync().ConfigureAwait(false));
        }

        logger.LogInformation("The email sender Quartz job finished.");
    }

    private string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);

        string returnValue = Encoding.ASCII.GetString(encodedDataAsBytes);

        return returnValue;
    }
}
