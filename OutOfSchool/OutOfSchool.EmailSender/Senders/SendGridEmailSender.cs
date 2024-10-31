using System;
using System.Net;
using System.Threading.Tasks;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender.Senders;

/// <summary>
/// A service that sends emails using the SendGrid API.
/// </summary>
/// <exception cref="JobExecutionException">
/// Thrown if the email sending rate limit is exceeded.
/// </exception>
/// <exception cref="Exception">
/// Thrown with the error message from SendGrid if the email fails to send for any other reason.
/// </exception>
/// <remarks>
/// This service is implemented using the SendGrid API client to send emails.
/// </remarks>
public class SendGridEmailSender(ISendGridClient sendGridClient) : IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously using the SendGrid API.
    /// </summary>
    /// <param name="sendGridMessage">The SendGrid message containing the email details.</param>
    /// <exception cref="JobExecutionException">
    /// Thrown when the SendGrid API returns a status code indicating that the rate limit has been exceeded.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the SendGrid API returns a failure status code for reasons other than rate limiting, including an error message from the API response.
    /// </exception>
    public async Task SendAsync(SendGridMessage sendGridMessage)
    {
        var response = await sendGridClient.SendEmailAsync(sendGridMessage).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new JobExecutionException("Email sending rate limit exceeded.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var sendError = await response.Body.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"Email was not sent with the following error: {sendError}");
        }
    }
}