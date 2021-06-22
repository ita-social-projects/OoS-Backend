using System.Threading.Tasks;

namespace OutOfSchool.EmailSender
{
    public interface IEmailSender
    {
        Task SendAsync(Message message);

        Task SendAsync(string email, string subject, string htmlMessage);
    }
}
