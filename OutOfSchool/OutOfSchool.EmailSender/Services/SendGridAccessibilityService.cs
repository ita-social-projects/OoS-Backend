using System;
using Microsoft.Extensions.Options;

namespace OutOfSchool.EmailSender.Services;

public class SendGridAccessibilityService : ISendGridAccessibilityService
{
    private readonly IOptions<EmailOptions> emailOptions;
    private const int defaultTimeoutTime = 240;

    public SendGridAccessibilityService(IOptions<EmailOptions> emailOptions)
    {
        this.emailOptions = emailOptions;
    }

    private DateTimeOffset accessibleAfter = DateTimeOffset.Now;

    public void SetSendGridInaccessible(DateTimeOffset now)
      => accessibleAfter = now + TimeSpan.FromMinutes(emailOptions.Value.TimeoutTime ?? defaultTimeoutTime);

    public bool IsSendGridAccessible(DateTimeOffset now)
      => accessibleAfter <= now;
}
