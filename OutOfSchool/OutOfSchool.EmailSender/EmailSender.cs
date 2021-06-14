using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace OutOfSchool.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly string _server;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public EmailSender(IOptions<SmtpOptions> options)
        {
            _server = options.Value.Server;
            _port = options.Value.Port;
            _username = options.Value.Username;
            _password = options.Value.Password;
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
                await emailClient.ConnectAsync(_server, _port, true);
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await emailClient.AuthenticateAsync(_username, _password);
                await emailClient.SendAsync(mimeMessage);
                await emailClient.DisconnectAsync(true);
            }
        }
    }
}
