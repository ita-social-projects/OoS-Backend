using System;
using System.Threading.Tasks;

namespace OutOfSchool.EmailSender;

public interface IEmailSenderService
{
    Task SendAsync(string email, string subject, (string html, string plain) content, DateTime expirationTime);
}