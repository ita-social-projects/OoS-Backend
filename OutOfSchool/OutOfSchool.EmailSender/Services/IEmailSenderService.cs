using System;
using System.Threading.Tasks;

namespace OutOfSchool.EmailSender.Services;

public interface IEmailSenderService
{
    Task SendAsync(string email, string subject, (string html, string plain) content, DateTime? expirationTime = null);
}