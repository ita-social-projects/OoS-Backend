using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub
    {
        // This collection tracks users with their connections.
        private static readonly ConcurrentDictionary<string, HubUser> Users
            = new ConcurrentDictionary<string, HubUser>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ILogger<ChatHub> logger;
        private readonly IChatMessageService messageService;
        private readonly IChatRoomService roomService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHub"/> class.
        /// </summary>
        /// <param name="chatMessageService">Service for ChatMessage model.</param>
        /// <param name="chatRoomService">Service for ChatRoom model.</param>
        /// <param name="logger">Logger.</param>
        public ChatHub(ILogger<ChatHub> logger, IChatMessageService chatMessageService, IChatRoomService chatRoomService)
        {
            this.logger = logger;
            this.messageService = chatMessageService;
            this.roomService = chatRoomService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.LogInformation($"New Hub-connection established. UserId: {userId}");

            this.AddUsersConnectionIdTracking(userId);

            // Add User to all Groups where he is a member.
            var usersRooms = await roomService.GetByUserId(userId).ConfigureAwait(false);
            foreach (var room in usersRooms)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
            }

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.LogInformation($"UserId: {userId} connection:{Context.ConnectionId} disconnected.");

            this.RemoveUsersConnectionIdTracking(userId);

            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a <see cref="ChatMessageDto"/>, saves it to DataBase and sends message to Others in Group.
        /// </summary>
        /// <param name="chatNewMessage">Entity (string format) that contains text of message, receiver and workshop info.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task SendMessageToOthersInGroup(string chatNewMessage)
        {
            ChatNewMessageDto chatNewMessageDto = null;
            try
            {
                chatNewMessageDto = JsonConvert.DeserializeObject<ChatNewMessageDto>(chatNewMessage);
            }
            catch (JsonReaderException exception)
            {
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", exception.Message).ConfigureAwait(false);
                throw;
            }

            var senderUserId = Context.User.FindFirst("sub")?.Value;
            logger.LogInformation($"{nameof(SendMessageToOthersInGroup)}.Invoked.");

            var chatMessageDto = new ChatMessageDto()
            {
                UserId = senderUserId,
                ChatRoomId = 0,
                Text = chatNewMessageDto.Text,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = false,
            };

            bool roomIsNew = false;

            // Validate chat between users and get chatRoom.
            if (chatNewMessageDto.ChatRoomId > 0 &&
                (await this.RoomExistAndSenderIsItsParticipant(chatNewMessageDto.ChatRoomId, senderUserId).ConfigureAwait(false)))
            {
                chatMessageDto.ChatRoomId = chatNewMessageDto.ChatRoomId;
            }
            else
            {
                var chatIsPossible = await roomService.UsersCanChatBetweenEachOther(senderUserId, chatNewMessageDto.ReceiverUserId, chatNewMessageDto.WorkshopId).ConfigureAwait(false);
                if (!chatIsPossible)
                {
                    await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", "Chat is forbidden between these users.").ConfigureAwait(false);
                    return;
                }
                else
                {
                    var chatRoomDto = await roomService.CreateOrReturnExisting(
                                        senderUserId, chatNewMessageDto.ReceiverUserId, chatNewMessageDto.WorkshopId)
                                        .ConfigureAwait(false);

                    chatMessageDto.ChatRoomId = chatRoomDto.Id;

                    roomIsNew = true;
                }
            }

            // Save chatMessage in the system.
            var createdMessageDto = await messageService.Create(chatMessageDto).ConfigureAwait(false);

            if (roomIsNew)
            {
                // Add Sender's connections to the Group.
                await AddConnectionsToGroup(senderUserId, createdMessageDto.ChatRoomId).ConfigureAwait(false);

                // Add Receiver's connections to the Group if he is online.
                await AddConnectionsToGroup(chatNewMessageDto.ReceiverUserId, createdMessageDto.ChatRoomId).ConfigureAwait(false);
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
            Users.TryGetValue(userId, out HubUser user);

            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId, StringComparison.Ordinal));

                    if (!user.ConnectionIds.Any())
                    {
                        Users.TryRemove(userId, out HubUser removedUser);
                    }
                }
            }
        }

        private async Task AddConnectionsToGroup(string userId, long chatRoomId)
        {
            if (Users.TryGetValue(userId, out HubUser user))
            {
                foreach (var connection in user.ConnectionIds)
                {
                    await Groups.AddToGroupAsync(connection, chatRoomId.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> RoomExistAndSenderIsItsParticipant(long chatRoomId, string senderId)
        {
            var usersRooms = await roomService.GetByUserId(senderId).ConfigureAwait(false);
            var room = usersRooms.Where(room => room.Id == chatRoomId).FirstOrDefault();
            if (room is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private class HubUser
        {
            public string UserId { get; set; }

            public HashSet<string> ConnectionIds { get; set; }
        }
    }
}
