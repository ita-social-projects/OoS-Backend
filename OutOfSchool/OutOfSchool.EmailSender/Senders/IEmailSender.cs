using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

public interface IEmailSender
{
    Task SendAsync(SendGridMessage sendGridMessage);
}