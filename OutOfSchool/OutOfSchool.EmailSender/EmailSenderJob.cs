using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender;
public class EmailSenderJob : IJob
{
    private readonly IOptions<EmailOptions> emailOptions;
    private readonly IEmailOutboxRepository outboxRepository;
    private readonly ILogger<EmailSenderJob> logger;
    private readonly ISendGridClient sendGridClient;

    public EmailSenderJob(
        IOptions<EmailOptions> emailOptions,
        IEmailOutboxRepository outboxRepository,
        ILogger<EmailSenderJob> logger,
        ISendGridClient sendGridClient)
    {
        this.emailOptions = emailOptions;
        this.outboxRepository = outboxRepository;
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

        var emailsToSend = outboxRepository.Get(orderBy: new() { { x => x.CreationTime, SortDirection.Ascending } });
        foreach (var email in emailsToSend)
        {
            if(email.ExpirationTime > DateTime.UtcNow)
            {
                await outboxRepository.Delete(email);
                logger.LogError("Email was not sent because expiration time passed: {Email}, {ExpirationTime}", email.Email, email.ExpirationTime);
            }

            var message = new SendGridMessage()
            {
                From = new EmailAddress()
                {
                    Email = emailOptions.Value.AddressFrom,
                    Name = emailOptions.Value.NameFrom,
                },
                Subject = email.Subject,
                HtmlContent = DecodeFrom64(email.HtmlContent),
                PlainTextContent = DecodeFrom64(email.PlainContent),
            };

            message.AddTo(new EmailAddress(email.Email));

            var response = await sendGridClient.SendEmailAsync(message).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogError("Email sending rate limit exceeded.");
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Email was not sent with the following error: {Error}", await response.Body.ReadAsStringAsync().ConfigureAwait(false));
                continue;
            }

            await outboxRepository.Delete(email);
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
