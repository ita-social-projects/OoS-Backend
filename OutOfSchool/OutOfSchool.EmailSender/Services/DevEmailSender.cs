using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.EmailSender.Services;

public class DevEmailSender : IEmailSenderService
{
    private readonly ILogger<DevEmailSender> logger;

    public DevEmailSender(ILogger<DevEmailSender> logger)
    {
        this.logger = logger;
    }

    public Task SendAsync(string email, string subject, (string html, string plain) content, DateTimeOffset? expirationTime = null)
    {
        expirationTime ??= DateTimeOffset.MaxValue;
        // This code runs only in dev for testing purposes
        // so can ignore "not log user-controlled data".
#pragma warning disable S5145
        logger.LogDebug(
            "Sending mail to {Email} with subject '{Subject}' and content: {Content} with expirationTime: {ExpirationTime}",
            email,
            subject,
            content.html,
            expirationTime);
#pragma warning restore S5145
        return Task.CompletedTask;
    }
}