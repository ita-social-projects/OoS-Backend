using System.Threading.Tasks;

namespace OutOfSchool.EmailService
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
        Task SendAsync(EmailMessage emailMessage);
    }
}
