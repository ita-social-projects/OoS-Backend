using System.Collections.Generic;

namespace OutOfSchool.EmailService
{
    public interface IEmailService
    {
        void Send(EmailMessage emailMessage);
    }
}
