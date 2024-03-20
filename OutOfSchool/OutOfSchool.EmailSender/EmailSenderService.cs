using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender;

public class EmailSenderService : IEmailSenderService
{
    private readonly IOptions<EmailOptions> emailOptions;
    private readonly ISendGridClient sendGridClient;
    private readonly ILogger<EmailSenderService> logger;
    private readonly IEmailOutboxRepository emailOutboxRepository;

    public EmailSenderService(
        IOptions<EmailOptions> emailOptions,
        ISendGridClient sendGridClient,
        ILogger<EmailSenderService> logger,
        IEmailOutboxRepository emailOutboxRepository)
    {
        this.emailOptions = emailOptions;
        this.sendGridClient = sendGridClient;
        this.logger = logger;
        this.emailOutboxRepository = emailOutboxRepository;
    }

    public async Task SendAsync(string email, string subject, (string html, string plain) content)
    {
        var outboxMessage = new EmailOutbox()
        {
            Email = email,
            Subject = subject,
            HtmlContent = content.html,
            PlainContent = content.plain,
            CreationTime = DateTime.Now,
        };
        await emailOutboxRepository.Create(outboxMessage);
        //var message = new SendGridMessage()
        //{
        //    From = new EmailAddress()
        //    {
        //        Email = emailOptions.Value.AddressFrom,
        //        Name = emailOptions.Value.NameFrom,
        //    },
        //    Subject = subject,
        //    HtmlContent = content.html,
        //    PlainTextContent = content.plain,
        //};
        //message.AddTo(new EmailAddress(email));

        //return SendAsync(message);
    }

    private async Task SendAsync(SendGridMessage message)
    {
        if (!emailOptions.Value.Enabled)
        {
            return;
        }

        try
        {
            var response = await sendGridClient.SendEmailAsync(message).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Email was not sent with the following error: {Error}", await response.Body.ReadAsStringAsync().ConfigureAwait(false));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Email was not sent due to an exception or reading response body failed");
        }
    }
}