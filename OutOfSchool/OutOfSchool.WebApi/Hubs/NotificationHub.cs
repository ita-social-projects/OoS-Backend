using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            string name = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            await Groups.AddToGroupAsync(Context.ConnectionId, name).ConfigureAwait(false);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }
    }
}
