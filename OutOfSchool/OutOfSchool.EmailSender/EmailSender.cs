using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace OutOfSchool.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpConfiguration _smtpConfiguration;

        public EmailSender(SmtpConfiguration smtpConfiguration)
        {
            _smtpConfiguration = smtpConfiguration;
        }

        public async Task SendAsync(Message message)
        {
            var mimeMessage = CreateMimeMessage(message);
            await SendAsync(mimeMessage);
        }

        private MimeMessage CreateMimeMessage(Message message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.To.Add(new MailboxAddress(message.To.Name, message.To.Address));
            mimeMessage.From.Add(new MailboxAddress(message.From.Name, message.From.Address));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message.Content
            };
            return mimeMessage;
        }

        private async Task SendAsync(MimeMessage mimeMessage)
        {
            using (var emailClient = new SmtpClient())
            {
                await emailClient.ConnectAsync(_smtpConfiguration.Server, _smtpConfiguration.Port, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await emailClient.AuthenticateAsync(_smtpConfiguration.Username, _smtpConfiguration.Password);
                await emailClient.SendAsync(mimeMessage);
                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
