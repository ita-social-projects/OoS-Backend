using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Hubs
{
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

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message).ConfigureAwait(false);
        }
    }
}
