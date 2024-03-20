using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using Quartz;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace OutOfSchool.EmailSender;
public class EmailSender : IJob
{
    private readonly IOptions<EmailOptions> emailOptions;
    private readonly IEmailOutboxRepository outboxRepository;
    private readonly ILogger<EmailSender> logger;
    private readonly ISendGridClient sendGridClient;

    public EmailSender(
        IOptions<EmailOptions> emailOptions,
        IEmailOutboxRepository outboxRepository,
        ILogger<EmailSender> logger,
        ISendGridClient sendGridClient)
    {
        this.emailOptions = emailOptions;
        this.outboxRepository = outboxRepository;
        this.logger = logger;
        this.sendGridClient = sendGridClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var emailsToSend = outboxRepository.Get(orderBy: new() { { x => x.CreationTime, SortDirection.Ascending } });
        foreach (var email in emailsToSend)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress()
                {
                    Email = emailOptions.Value.AddressFrom,
                    Name = emailOptions.Value.NameFrom,
                },
                Subject = email.Subject,
                HtmlContent = email.HtmlContent,
                PlainTextContent = email.PlainContent,
            };

            message.AddTo(new EmailAddress(email.Email));

            var response = await sendGridClient.SendEmailAsync(message).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Email was not sent with the following error: {Error}", await response.Body.ReadAsStringAsync().ConfigureAwait(false));
                continue;
            }

            await outboxRepository.Delete(email);
        }
    }
}
