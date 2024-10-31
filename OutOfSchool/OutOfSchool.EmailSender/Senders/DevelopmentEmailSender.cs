using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

public class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    public Task SendAsync(SendGridMessage sendGridMessage)
    {
        string expirationTime = sendGridMessage.CustomArgs["expirationTime"] ?? DateTimeOffset.Now.ToString();

        logger.LogDebug(
            "Sending mail to {Email} with subject '{Subject}' and content: {Content} with expirationTime: {ExpirationTime}",
            sendGridMessage.ReplyTo.Email,
            sendGridMessage.Subject,
            sendGridMessage.HtmlContent,
            expirationTime);

        return Task.CompletedTask;
    }
}