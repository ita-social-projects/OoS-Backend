using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

/// <summary>
/// Defines a contract for email-sending functionality.
/// </summary>
/// <remarks>
/// Implementations of this interface provide a mechanism to send emails.
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="sendGridMessage">The message containing email details such as recipient, subject, content etc.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
    Task SendAsync(SendGridMessage sendGridMessage);
}