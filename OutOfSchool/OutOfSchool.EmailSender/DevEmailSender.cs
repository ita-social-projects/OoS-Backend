using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.EmailSender;

public class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> logger;

    public DevEmailSender(ILogger<DevEmailSender> logger)
    {
        this.logger = logger;
    }
        
    public Task SendAsync(string email, string subject, (string html, string plain) content)
    {
        // This code runs only in dev for testing purposes
        // so can ignore "not log user-controlled data".
        #pragma warning disable S5145
        logger.LogDebug(
            "Sending mail to {Email} with subject '{Subject}' and content: {Content}",
            email,
            subject,
            content.html);
        #pragma warning restore S5145
        return Task.CompletedTask;
    }
}