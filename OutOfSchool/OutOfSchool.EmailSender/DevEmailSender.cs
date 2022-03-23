using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.EmailSender
{
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> logger;

        public DevEmailSender(ILogger<DevEmailSender> logger)
        {
            this.logger = logger;
        }
        
        public Task SendAsync(string email, string subject, string htmlMessage)
        {
            logger.LogDebug($"Sending mail to {email} with subject \"{subject}\" and content: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}