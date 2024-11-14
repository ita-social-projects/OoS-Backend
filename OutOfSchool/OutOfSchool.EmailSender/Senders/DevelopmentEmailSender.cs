using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

/// <summary>
/// A development-only implementation of <see cref="IEmailSender"/> that logs email details instead of sending them.
/// </summary>
/// <remarks>
/// This service is intended for development environments where actual email sending is not required.
/// It logs the email recipient, subject, content, and expiration time for debugging purposes.
/// </remarks>
public class DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    /// <summary>
    /// Logs the email details instead of sending it.
    /// </summary>
    /// <param name="sendGridMessage">The SendGrid message containing the email details.</param>
    /// <remarks>
    /// This method logs the recipient email address, subject, HTML content, and expiration time.
    /// <list type="bullet">
    /// <item><description>If an expiration time is provided in <c>CustomArgs["expirationTime"]</c>, it will be logged.</description></item>
    /// <item><description>If no expiration time is provided, the current time will be used as a default value.</description></item>
    /// </list>
    /// </remarks>
    /// <returns>A completed <see cref="Task"/>.</returns>
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