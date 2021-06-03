using System.Threading.Tasks;

namespace OutOfSchool.EmailService
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage emailMessage);
    }
}
