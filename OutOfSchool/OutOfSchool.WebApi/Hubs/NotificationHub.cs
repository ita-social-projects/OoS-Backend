using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;

            await Groups.AddToGroupAsync(Context.ConnectionId, name).ConfigureAwait(false);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }
    }
}
