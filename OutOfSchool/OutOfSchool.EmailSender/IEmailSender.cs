using System.Threading.Tasks;

namespace OutOfSchool.EmailSender;

public interface IEmailSender
{
    Task SendAsync(string email, string subject, (string html, string plain) content);
}