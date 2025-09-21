using OutOfSchool.Common.Models;
using System.Security.Claims;

namespace OutOfSchool.BusinessLogic.Services;
/// <summary>
/// Returns id of current web user, or system user id if running in system context.
/// </summary>
public class ContextAwareUserService : IContextAwareUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public ContextAwareUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public bool IsSystemContext => !(Principal?.Identity?.IsAuthenticated ?? false);

    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string UserId
    {
        get
        {
            var user = Principal;
            if (user?.Identity?.IsAuthenticated is true)
            {
                var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }

            return Constants.SystemUserConstants.SystemUserId;
        }
    }
}
