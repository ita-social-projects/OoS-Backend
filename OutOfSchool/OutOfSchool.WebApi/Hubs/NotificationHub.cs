using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationHub"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public NotificationHub(
            ILogger<NotificationHub> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.logger = logger;
            this.localizer = localizer;
        }

        public override async Task OnConnectedAsync()
        {
            string name = Context.User.Identity.Name;

            await Groups.AddToGroupAsync(Context.ConnectionId, name).ConfigureAwait(false);

            await base.OnConnectedAsync().ConfigureAwait(false);
        }
    }
}
