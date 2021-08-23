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
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class ChatWorkshopHub : Hub
    {
        // This collection tracks users with their connections.
        private static readonly ConcurrentDictionary<string, HashSet<string>> UsersConnections
            = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ILogger<ChatWorkshopHub> logger;
        private readonly IChatMessageWorkshopService messageService;
        private readonly IChatRoomWorkshopService roomService;
        private readonly IProviderService providerService;
        private readonly IParentService parentService;
        private readonly IWorkshopService workshopService;

        private ParentDTO parent;
        private ProviderDto provider;
        private WorkshopDTO workshop;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWorkshopHub"/> class.
        /// </summary>
        /// <param name="chatMessageService">Service for ChatMessage entities.</param>
        /// <param name="chatRoomService">Service for ChatRoom entities.</param>
        /// <param name="providerService">Service for Provider entities.</param>
        /// <param name="parentService">Service for Parent entities.</param>
        /// <param name="workshopService">Service for Workshop entities.</param>
        /// <param name="logger">Logger.</param>
        public ChatWorkshopHub(ILogger<ChatWorkshopHub> logger, IChatMessageWorkshopService chatMessageService, IChatRoomWorkshopService chatRoomService, IProviderService providerService, IParentService parentService, IWorkshopService workshopService)
        {
            this.logger = logger;
            this.messageService = chatMessageService;
            this.roomService = chatRoomService;
            this.providerService = providerService;
            this.parentService = parentService;
            this.workshopService = workshopService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.LogInformation($"New Hub-connection established. UserId: {userId}");

            this.AddUsersConnectionIdTracking(userId);

            // Add User to all Groups where he is a member.
            IEnumerable<ChatRoomWorkshopDtoWithLastMessage> usersRooms;
            if (Role.Provider.ToString().Equals(Context.User.FindFirst("role")?.Value, StringComparison.OrdinalIgnoreCase))
            {
                var providerLocal = await providerService.GetByUserId(userId).ConfigureAwait(false);
                usersRooms = await roomService.GetByProviderIdAsync(providerLocal.Id).ConfigureAwait(false);
            }
            else
            {
                var parentLocal = await parentService.GetByUserId(userId).ConfigureAwait(false);
                usersRooms = await roomService.GetByParentIdAsync(parentLocal.Id).ConfigureAwait(false);
            }

            foreach (var room in usersRooms)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);
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
        /// Creates a <see cref="ChatMessageWorkshopDto"/>, saves it to the DataBase and sends message to Others in Group.
        /// </summary>
        /// <param name="chatNewMessage">Entity (string format) that contains text of message, receiver and workshop info.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task SendMessageToOthersInGroupAsync(string chatNewMessage)
        {
            logger.LogInformation($"{nameof(SendMessageToOthersInGroupAsync)}.Invoked.");

            try
            {
                // deserialize from string to Object
                ChatMessageWorkshopCreateDto newReceivedAndDeserializedMessage = JsonConvert.DeserializeObject<ChatMessageWorkshopCreateDto>(chatNewMessage);

                // validate received parameters
                var messageIsValid = await this.ValidateNewMessage(newReceivedAndDeserializedMessage).ConfigureAwait(false);
                if (!messageIsValid)
                {
                    string message = "Some of the message parameters were wrong. There are no entities with such Ids or Ids do not belong to sender.";
                    await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", message).ConfigureAwait(false);
                    return;
                }

                // create new dto object that will be saved to the database
                var chatMessageDtoThatWillBeSaved = new ChatMessageWorkshopDto()
                {
                    SenderRoleIsProvider = newReceivedAndDeserializedMessage.SenderRoleIsProvider,
                    Text = newReceivedAndDeserializedMessage.Text,
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsRead = false,
                    ChatRoomId = 0,
                };

                bool roomIsNew = false;
                var existingRoom = await roomService.GetUniqueChatRoomAsync(newReceivedAndDeserializedMessage.WorkshopId, newReceivedAndDeserializedMessage.ParentId)
                    .ConfigureAwait(false);

                // set the unique ChatRoomId property according to WorkshopId and ParentId
                if (existingRoom is null)
                {
                    var newChatRoomDto = await roomService.CreateOrReturnExistingAsync(newReceivedAndDeserializedMessage.WorkshopId, newReceivedAndDeserializedMessage.ParentId)
                        .ConfigureAwait(false);
                    chatMessageDtoThatWillBeSaved.ChatRoomId = newChatRoomDto.Id;
                    roomIsNew = true;
                }
                else
                {
                    chatMessageDtoThatWillBeSaved.ChatRoomId = existingRoom.Id;
                }

                // Save chatMessage in the system.
                var createdMessageDto = await messageService.CreateAsync(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);

                if (roomIsNew)
                {
                    // Add Parent's connections to the Group if he is online.
                    await AddConnectionsToGroupAsync(parent.UserId, createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);

                    // Add Provider's connections to the Group if he is online.
                    if (provider is null)
                    {
                        provider = await providerService.GetById(workshop.ProviderId).ConfigureAwait(false);
                        await AddConnectionsToGroupAsync(provider.UserId, createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);
                    }
                    else
                    {
                        await AddConnectionsToGroupAsync(provider.UserId, createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);
                    }
                }

                // Send chatMessage.
                await Clients.OthersInGroup(createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat")
                    .SendAsync("ReceiveMessageInChatGroup", JsonConvert.SerializeObject(createdMessageDto))
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", exception.Message).ConfigureAwait(false);
            }
        }

        private void AddUsersConnectionIdTracking(string userId)
        {
            var connectionIds = UsersConnections.GetOrAdd(userId, _ => new HashSet<string>());

            lock (connectionIds)
            {
                connectionIds.Add(Context.ConnectionId);
            }
        }

        private void RemoveUsersConnectionIdTracking(string userId)
        {
            bool result = UsersConnections.TryGetValue(userId, out HashSet<string> connectionIds);

            if (result)
            {
                lock (connectionIds)
                {
                    connectionIds.RemoveWhere(cid => cid.Equals(Context.ConnectionId, StringComparison.Ordinal));

                    // if User has no more any ConnectionIds then delete it from dictionary
                    if (!connectionIds.Any())
                    {
                        UsersConnections.TryRemove(userId, out HashSet<string> removedConnectionIds);
                    }
                }
            }
        }

        private async Task AddConnectionsToGroupAsync(string userId, string chatRoomUniqueName)
        {
            if (UsersConnections.TryGetValue(userId, out HashSet<string> connectionIds))
            {
                foreach (var connection in connectionIds)
                {
                    await Groups.AddToGroupAsync(connection, chatRoomUniqueName).ConfigureAwait(false);
                }
            }
        }

        private async Task SetProviderAndParentAndWorkshopFields(ChatMessageWorkshopCreateDto newMessage)
        {
            if (newMessage is null)
            {
                throw new ArgumentNullException($"{nameof(newMessage)}");
            }

            var userRole = Context.User.FindFirst("role")?.Value;
            if (Role.Provider.ToString().Equals(userRole, StringComparison.OrdinalIgnoreCase))
            {
                // if Sender role is Provider
                // so all fields need to be settled for future validation if provider is workshop's owner
                provider = await providerService.GetByUserId(Context.User.FindFirst("sub")?.Value).ConfigureAwait(false);
                workshop = await workshopService.GetById(newMessage.WorkshopId).ConfigureAwait(false);
                parent = await parentService.GetById(newMessage.ParentId).ConfigureAwait(false);

                if (provider is null || parent is null || workshop is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(provider)}, {nameof(parent)}, {nameof(workshop)}");
                }
            }
            else
            {
                // if Sender role is Parent
                // so only workshop and parent need to be checked if they exist in the system and Message.ParentId is right
                parent = await parentService.GetByUserId(Context.User.FindFirst("sub")?.Value).ConfigureAwait(false);
                workshop = await workshopService.GetById(newMessage.WorkshopId).ConfigureAwait(false);

                if (parent is null || workshop is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(parent)}, {nameof(workshop)}");
                }
            }
        }

        private async Task<bool> ValidateNewMessage(ChatMessageWorkshopCreateDto newMessage)
        {
            if (newMessage is null)
            {
                throw new ArgumentNullException($"{nameof(newMessage)}");
            }

            await this.SetProviderAndParentAndWorkshopFields(newMessage).ConfigureAwait(false);

            if (provider is null)
            {
                // if provider is not set to instance, so the sender is Parent
                // and we check if ParentId is equal to Sender Parent.Id
                // and SenderRole must be Parent
                if (newMessage.SenderRoleIsProvider || newMessage.ParentId != parent.Id)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                // if provider is set to instance, so the sender is Provider
                // and we check if workshop is initialized and has ProviderId equal Sender Provider.Id
                // and SenderRole must be Provider
                if (workshop.ProviderId != provider.Id || !newMessage.SenderRoleIsProvider)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
