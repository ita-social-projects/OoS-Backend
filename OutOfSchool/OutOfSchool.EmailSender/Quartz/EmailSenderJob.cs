using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Quartz;

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
        try 
        { 
            logger.LogInformation("The email sender Quartz job started.");

            if (!emailOptions.Value.Enabled)
            {
                logger.LogError("The email sender is disabled.");
                return;
            }

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var email = dataMap.GetString(EmailSenderStringConstants.Email);
            var subject = dataMap.GetString(EmailSenderStringConstants.Subject);
            var htmlContent = dataMap.GetString(EmailSenderStringConstants.HtmlContent);
            var plainContent = dataMap.GetString(EmailSenderStringConstants.PlainContent);
            var expirationTime = DateTimeOffset.Parse(dataMap.GetString(EmailSenderStringConstants.ExpirationTime), CultureInfo.CurrentCulture);

            if (expirationTime < DateTimeOffset.Now)
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
                throw new JobExecutionException("Email sending rate limit exceeded.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Email was not sent with the following error: {await response.Body.ReadAsStringAsync().ConfigureAwait(false)}");
            }

            logger.LogInformation("The email sender Quartz job finished.");
        }
        catch (JobExecutionException ex)
        {
            logger.LogError(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    private string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);

        string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);

        return returnValue;
    }
}
