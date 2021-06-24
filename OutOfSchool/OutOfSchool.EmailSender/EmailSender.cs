using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace OutOfSchool.EmailSender
{
    public class EmailSender : IEmailSender
    {
        IOptions<EmailOptions> _emailOptions;
        IOptions<SmtpOptions> _smtpOptions;

        public EmailSender(IOptions<EmailOptions> emailOptions, IOptions<SmtpOptions> smtpOptions)
        {
            _emailOptions = emailOptions;
            _smtpOptions = smtpOptions;
        }

        public Task SendAsync(Message message)
        {
            var mimeMessage = CreateMimeMessage(message);
            return SendAsync(mimeMessage);
        }

        public Task SendAsync(string email, string subject, string htmlMessage)
        {
            var message = new Message()
            {
                From = new EmailAddress()
                {
                    Name = _emailOptions.Value.NameFrom,
                    Address = _emailOptions.Value.AddressFrom,
                },
                To = new EmailAddress()
                {
                    Name = email,
                    Address = email,
                },
                Content = htmlMessage,
                Subject = subject,
            };
            return SendAsync(message);
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
            if (!_emailOptions.Value.Enabled)
                return;
                
            using (var emailClient = new SmtpClient())
            {
                await emailClient.ConnectAsync(_smtpOptions.Value.Server, _smtpOptions.Value.Port, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await emailClient.AuthenticateAsync(_smtpOptions.Value.Username, _smtpOptions.Value.Password);
                await emailClient.SendAsync(mimeMessage);
                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
