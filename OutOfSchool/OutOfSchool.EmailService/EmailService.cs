using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace OutOfSchool.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;
        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }
        public void Send(EmailMessage emailMessage)
        {
            var mimeMessage = CreateMimeMessage(emailMessage);
            Send(mimeMessage);
        }
        public async Task SendAsync(EmailMessage emailMessage)
        {
            var mimeMessage = CreateMimeMessage(emailMessage);
            await SendAsync(mimeMessage);
        }
        private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };
            return message;
        }
        private void Send(MimeMessage mimeMessage)
        {
            using(var emailClient = new SmtpClient())
            {
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                emailClient.Send(mimeMessage);
                emailClient.Disconnect(true);
            }
        }
        private async Task SendAsync(MimeMessage mimeMessage)
        {
            using (var emailClient = new SmtpClient())
            {
                await emailClient.ConnectAsync(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await emailClient.AuthenticateAsync(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                await emailClient.SendAsync(mimeMessage);
                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
