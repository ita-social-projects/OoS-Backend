using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub
    {
        // TODO: change static collection with a persistent storage system such as MongoDB, RavenDB, SQL Server, etc.
        // References:
        // https://www.tugberkugurlu.com/archive/mapping-asp-net-signalr-connections-to-real-application-users
        // https://github.com/davidfowl/MessengR/blob/master/MessengR/Hubs/Chat.cs
        // This collection tracks users with their connections.
        private static readonly ConcurrentDictionary<string, HubUser> Users
            = new ConcurrentDictionary<string, HubUser>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ILogger logger;
        private readonly IChatMessageService messageService;
        private readonly IChatRoomService roomService;

        public ChatHub(ILogger logger, IChatMessageService chatMessageService, IChatRoomService chatRoomService)
        {
            this.logger = logger;
            this.messageService = chatMessageService;
            this.roomService = chatRoomService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.Information($"New Hub-connection established. UserId: {userId}");

            this.AddUsersConnectionIdTracking(userId);

            // Add User to all Groups where he is a member.
            var usersRooms = await roomService.GetByUserId(userId).ConfigureAwait(false);
            foreach (var room in usersRooms)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
            }

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.Information($"UserId: {userId} connection:{Context.ConnectionId} disconnected.");

            this.RemoveUsersConnectionIdTracking(userId);
            
            await base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// Creates <see cref="ChatMessageDTO"/>, saves to DataBase and sends message to Others in Group.
        /// </summary>
        /// <param name="chatNewMessage">Entity (string format) that contains text of message, receiver and workshop info.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task SendMessageToOthersInGroup(string chatNewMessage)
        {
            ChatNewMessageDTO chatNewMessageDtoObject = null;
            try
            {
                chatNewMessageDtoObject = JsonConvert.DeserializeObject<ChatNewMessageDTO>(chatNewMessage);
            }
            catch (JsonException ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", ex.Message).ConfigureAwait(false);
                throw;
            }

            var senderUserId = Context.User.FindFirst("sub")?.Value;
            logger.Information($"{nameof(SendMessageToOthersInGroup)}.Invoked UserId: {senderUserId}");

            var chatMessageDto = new ChatMessageDTO()
            {
                UserId = senderUserId,
                ChatRoomId = 0,
                Text = chatNewMessageDtoObject.Text,
                CreatedTime = DateTime.Now,
                IsRead = false,
            };

            bool roomIsNew = false;

            // Validate chat between users and get chatRoom.
            try
            {
                if (await this.ValidateChatRoomId(chatNewMessageDtoObject.ChatRoomId, senderUserId).ConfigureAwait(false))
                {
                    chatMessageDto.ChatRoomId = chatNewMessageDtoObject.ChatRoomId;
                }
                else
                {
                    await roomService.ValidateUsers(senderUserId, chatNewMessageDtoObject.ReceiverUserId, chatNewMessageDtoObject.WorkshopId).ConfigureAwait(false);

                    var chatRoomDto = await roomService.CreateOrReturnExisting(
                    senderUserId, chatNewMessageDtoObject.ReceiverUserId, chatNewMessageDtoObject.WorkshopId)
                    .ConfigureAwait(false);

                    chatMessageDto.ChatRoomId = chatRoomDto.Id;

                    roomIsNew = true;
                }
            }
            catch (ArgumentException ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", ex.Message).ConfigureAwait(false);
                throw;
            }

            // Save chatMessage in the system.
            var createdMessageDto = await messageService.Create(chatMessageDto).ConfigureAwait(false);

            if (roomIsNew)
            {
                // Add Sender's connections to the Group.
                await AddConnectionsToGroup(senderUserId, createdMessageDto.ChatRoomId).ConfigureAwait(false);

                // Add Receiver's connections to the Group if he is online.
                await AddConnectionsToGroup(chatNewMessageDtoObject.ReceiverUserId, createdMessageDto.ChatRoomId).ConfigureAwait(false);
            }

            // Send chatMessage.
            await Clients.OthersInGroup(createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture))
                .SendAsync("ReceiveMessageInChatGroup", JsonConvert.SerializeObject(createdMessageDto))
                .ConfigureAwait(false);
        }

        private void AddUsersConnectionIdTracking(string userId)
        {
            var hubUser = Users.GetOrAdd(userId, _ => new HubUser
            {
                UserId = userId,
                ConnectionIds = new HashSet<string>(),
            });

            lock (hubUser.ConnectionIds)
            {
                hubUser.ConnectionIds.Add(Context.ConnectionId);
            }
        }

        private void RemoveUsersConnectionIdTracking(string userId)
        {
            HubUser user;
            Users.TryGetValue(userId, out user);

            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId, StringComparison.Ordinal));

                    if (!user.ConnectionIds.Any())
                    {
                        HubUser removedUser;
                        Users.TryRemove(userId, out removedUser);
                    }
                }
            }
        }

        private async Task AddConnectionsToGroup(string userId, long chatRoomId)
        {
            HubUser user;
            if (Users.TryGetValue(userId, out user))
            {
                foreach (var connection in user.ConnectionIds)
                {
                    await Groups.AddToGroupAsync(connection, chatRoomId.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> ValidateChatRoomId(long chatRoomId, string senderId)
        {
            if (chatRoomId < 0)
            {
                throw new ArgumentException($"Wrong ChatRoom id:{chatRoomId}.");
            }

            if (chatRoomId > 0)
            {
                var existingChatRoom = await roomService.GetById(chatRoomId).ConfigureAwait(false);
                if (!(existingChatRoom is null) && existingChatRoom.Users.Any(u => u.Id == senderId))
                {
                    return true;
                }
                else
                {
                    throw new ArgumentException($"You are not a participant in ChatRoom with id:{chatRoomId}.");
                }
            }

            return false;
        }

        private class HubUser
        {
            public string UserId { get; set; }

            public HashSet<string> ConnectionIds { get; set; }
        }
    }
}
