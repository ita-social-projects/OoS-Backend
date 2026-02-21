using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.EmailSender.Senders;
using Quartz;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Quartz;

[DisallowConcurrentExecution]
public class EmailSenderJob : IJob
{
    private readonly IOptions<EmailOptions> emailOptions;
    private readonly ILogger<EmailSenderJob> logger;
    private readonly IEmailSender emailSender;
    
    public EmailSenderJob(
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailSenderJob> logger,
        IEmailSender emailSender)
    {
        this.emailOptions = emailOptions;
        this.logger = logger;
        this.emailSender = emailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try 
        { 
            logger.LogInformation("The email sender Quartz job started.");

            if (!context.MergedJobDataMap.Any())
            {
                logger.LogDebug("Default Quartz job with no data processing. Skipping.");
                return;
            }

            if (!emailOptions.Value.Enabled)
            {
                logger.LogError("The email sender is disabled.");
                return;
            }

            JobDataMap dataMap = context.MergedJobDataMap;

            var email = dataMap.GetString(EmailSenderStringConstants.Email);
            var subject = dataMap.GetString(EmailSenderStringConstants.Subject);
            var htmlContent = dataMap.GetString(EmailSenderStringConstants.HtmlContent);
            var plainContent = dataMap.GetString(EmailSenderStringConstants.PlainContent);
            var expirationTime = DateTimeOffset.ParseExact(
                dataMap.GetString(EmailSenderStringConstants.ExpirationTime) ?? string.Empty,
                EmailSenderStringConstants.DateTimeStringFormat,
                CultureInfo.InvariantCulture);

            if (expirationTime < DateTimeOffset.Now)
            {
                logger.LogError("Email was not sent because expiration time passed: {Email}, {ExpirationTime}", email, expirationTime);
                return;
            }

            var message = new SendGridMessage
            {
                From = new EmailAddress
                {
                    Email = emailOptions.Value.AddressFrom,
                    Name = emailOptions.Value.NameFrom,
                },
                Subject = subject,
                HtmlContent = DecodeFrom64(htmlContent),
                PlainTextContent = DecodeFrom64(plainContent),
            };

            message.AddTo(new EmailAddress(email));
            message.AddCustomArg(nameof(expirationTime), expirationTime.ToString());

            await emailSender.SendAsync(message);
            
            logger.LogInformation("The email sender Quartz job finished.");
        }
        catch (JobExecutionException ex)
        {
            ex.RefireImmediately = false;
            logger.LogError(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new JobExecutionException
            {
                RefireImmediately = false
            };
        }
    }

    private string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);

        string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);

        return returnValue;
    }
}
