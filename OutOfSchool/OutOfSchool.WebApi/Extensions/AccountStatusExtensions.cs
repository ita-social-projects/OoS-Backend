using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Extensions;

internal static class AccountStatusExtensions
{
    public static AccountStatus Convert(bool isBlocked, DateTimeOffset lastLogin)
        => isBlocked
            ? AccountStatus.Blocked
            : lastLogin == DateTimeOffset.MinValue
                ? AccountStatus.NeverLogged
                : AccountStatus.Accepted;

    public static AccountStatus Convert(User user)
        => Convert(user.IsBlocked, user.LastLogin);
}