using System;

namespace OutOfSchool.EmailSender.Services;

public interface ISendGridAccessibilityService
{
    bool IsSendGridAccessible { get; set; }
    DateTimeOffset? GetNextStartDate();
}
