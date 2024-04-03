using System;

namespace OutOfSchool.EmailSender.Services;

public class SendGridAccessibilityService : ISendGridAccessibilityService
{
    public SendGridAccessibilityService()
    {
        IsSendGridAccessible = true;
    }

    public bool IsSendGridAccessible { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Midnight of the next day is SendGrid accessible and null if it is not.</returns>
    public DateTimeOffset? GetNextStartDate()
    {
        if (IsSendGridAccessible)
        {
            return null;
        }
        return DateTimeOffset.Now.Date.AddDays(1);
    }
}
