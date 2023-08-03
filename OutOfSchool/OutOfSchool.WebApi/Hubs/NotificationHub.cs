﻿using Microsoft.AspNetCore.SignalR;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        string name = GettingUserProperties.GetUserId(Context.User);

        await Groups.AddToGroupAsync(Context.ConnectionId, name).ConfigureAwait(false);

        await base.OnConnectedAsync().ConfigureAwait(false);
    }
}