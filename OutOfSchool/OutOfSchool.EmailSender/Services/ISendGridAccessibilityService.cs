using System;

namespace OutOfSchool.EmailSender.Services;

public interface ISendGridAccessibilityService
{
    bool IsSendGridAccessible(DateTimeOffset now);
    void SetSendGridInaccessible(DateTimeOffset now);
}
