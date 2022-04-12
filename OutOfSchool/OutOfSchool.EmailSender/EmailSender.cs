using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<EmailOptions> emailOptions;
        private readonly ISendGridClient sendGridClient;
        private readonly ILogger<EmailSender> logger;

        public EmailSender(
            IOptions<EmailOptions> emailOptions,
            ISendGridClient sendGridClient,
            ILogger<EmailSender> logger)
        {
            this.emailOptions = emailOptions;
            this.sendGridClient = sendGridClient;
            this.logger = logger;
        }

        public Task SendAsync(string email, string subject, string htmlMessage)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress()
                {
                    Email = emailOptions.Value.AddressFrom,
                    Name = emailOptions.Value.NameFrom,
                },
                Subject = subject,
                //TODO: Add plaintext message fallback
                HtmlContent = htmlMessage
            };
            message.AddTo(new EmailAddress(email));

            return SendAsync(message);
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
                //TODO: Do Something with success?
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}
