using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.BusinessLogic.Extensions;

public static class AccountStatusExtensions
{
    public static AccountStatus Convert(bool isBlocked, DateTimeOffset lastLogin)
    {
        if (isBlocked)
        {
            return AccountStatus.Blocked;
        }

        return lastLogin == DateTimeOffset.MinValue
            ? AccountStatus.NeverLogged
            : AccountStatus.Accepted;
    }

    public static AccountStatus Convert(User user)
        => Convert(user.IsBlocked, user.LastLogin);
}