#nullable enable

using System.Security.Claims;

namespace OutOfSchool.Common.Models;
public interface IContextAwareCurrentUser : ICurrentUser
{
    /// <summary>
    /// True if there's no authenticated web-context.
    /// </summary>
    bool IsSystemContext { get; }

    ClaimsPrincipal? Principal { get; }
}