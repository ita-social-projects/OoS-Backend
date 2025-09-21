using System.Security.Claims;

namespace OutOfSchool.Common.Models;
public interface IContextAwareUserService
{
    /// <summary>
    /// Either id of user from web-context or system user id.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// True if there's no authenticated web-context.
    /// </summary>
    bool IsSystemContext { get; }

    ClaimsPrincipal? Principal { get; }
}
