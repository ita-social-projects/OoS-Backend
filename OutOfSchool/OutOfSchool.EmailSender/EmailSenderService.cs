using System;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.EmailSender;

public class EmailSenderService : IEmailSenderService
{
    private readonly IEmailOutboxRepository emailOutboxRepository;

    public EmailSenderService(
        IEmailOutboxRepository emailOutboxRepository)
    {
        this.emailOutboxRepository = emailOutboxRepository;
    }

    public async Task SendAsync(string email, string subject, (string html, string plain) content, DateTime expirationTime)
    {
        var outboxMessage = new EmailOutbox()
        {
            Email = email,
            Subject = subject,
            HtmlContent = EncodeToBase64(content.html),
            PlainContent = EncodeToBase64(content.plain),
            CreationTime = DateTime.Now,
            ExpirationTime = expirationTime
        };
        await emailOutboxRepository.Create(outboxMessage);
    }

    private string EncodeToBase64(string toEncode)
    {
        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);

        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}