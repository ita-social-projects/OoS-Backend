using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class ChatWorkshopHub : Hub
    {
        // This collection tracks users with their connections.
        private static readonly ConcurrentDictionary<string, HashSet<string>> UsersConnections
            = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ILogger logger;
        private readonly IChatMessageWorkshopService messageService;
        private readonly IChatRoomWorkshopService roomService;
        private readonly IValidationService validationService;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IParentRepository parentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWorkshopHub"/> class.
        /// </summary>
        /// <param name="chatMessageService">Service for ChatMessage entities.</param>
        /// <param name="chatRoomService">Service for ChatRoom entities.</param>
        /// <param name="validationService">Service for validation parameters.</param>
        /// <param name="workshopRepository">Repository for workshop entities.</param>
        /// <param name="parentRepository">Repository for parent entities.</param>
        /// <param name="logger">Logger.</param>
        public ChatWorkshopHub(ILogger logger, IChatMessageWorkshopService chatMessageService, IChatRoomWorkshopService chatRoomService, IValidationService validationService, IWorkshopRepository workshopRepository, IParentRepository parentRepository)
        {
            this.logger = logger;
            this.messageService = chatMessageService;
            this.roomService = chatRoomService;
            this.validationService = validationService;
            this.workshopRepository = workshopRepository;
            this.parentRepository = parentRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            var userRole = Context.User.FindFirst("role")?.Value;

            if (userId is null || userRole is null)
            {
                throw new ArgumentException($"{nameof(userId)} or {nameof(userRole)}");
            }

            logger.Information($"New Hub-connection established. UserId: {userId}");

            this.AddUsersConnectionIdTracking(userId);

            // Add User to all Groups where he is a member.
            IEnumerable<long> usersRoomIds;
            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);
            if (userRoleIsProvider)
            {
                var providerId = await validationService.GetEntityIdAccordingToUserRole(userId, userRole).ConfigureAwait(false);
                usersRoomIds = await roomService.GetChatRoomIdsByProviderIdAsync(providerId).ConfigureAwait(false);
            }
            else
            {
                var parentId = await validationService.GetEntityIdAccordingToUserRole(userId, userRole).ConfigureAwait(false);
                usersRoomIds = await roomService.GetChatRoomIdsByParentIdAsync(parentId).ConfigureAwait(false);
            }

            foreach (var id in usersRoomIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);
            }

            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.FindFirst("sub")?.Value;
            logger.Information($"UserId: {userId} connection:{Context.ConnectionId} disconnected.");

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
            logger.Information($"{nameof(SendMessageToOthersInGroupAsync)}.Invoked.");

            try
            {
                // deserialize from string to Object
                ChatMessageWorkshopCreateDto newReceivedAndDeserializedMessage = JsonConvert.DeserializeObject<ChatMessageWorkshopCreateDto>(chatNewMessage);

                // create new dto object that will be saved to the database
                var chatMessageDtoThatWillBeSaved = new ChatMessageWorkshopDto()
                {
                    SenderRoleIsProvider = newReceivedAndDeserializedMessage.SenderRoleIsProvider,
                    Text = newReceivedAndDeserializedMessage.Text,
                    CreatedDateTime = DateTimeOffset.UtcNow,
                    ReadDateTime = null,
                    ChatRoomId = 0,
                };

                bool userRoleIsProvider = string.Equals(Context.User.FindFirst("role")?.Value, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

                bool userHarRights = await this.UserHasRigtsForChatRoomAsync(newReceivedAndDeserializedMessage.WorkshopId, newReceivedAndDeserializedMessage.ParentId).ConfigureAwait(false);

                bool roomIsNew = false;

                if (userHarRights && (chatMessageDtoThatWillBeSaved.SenderRoleIsProvider == userRoleIsProvider))
                {
                    // set the unique ChatRoomId property according to WorkshopId and ParentId
                    var existingRoom = await roomService.GetUniqueChatRoomAsync(newReceivedAndDeserializedMessage.WorkshopId, newReceivedAndDeserializedMessage.ParentId)
                        .ConfigureAwait(false);

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
                }
                else
                {
                    string message = "Some of the message parameters were wrong. User has no rights for this chat room or sender role is set invalid.";
                    await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", message).ConfigureAwait(false);
                    return;
                }

                // Save chatMessage in the system.
                var createdMessageDto = await messageService.CreateAsync(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);

                if (roomIsNew)
                {
                    // Add Parent's connections to the Group if he is online.
                    var parent = await parentRepository.GetById(newReceivedAndDeserializedMessage.ParentId).ConfigureAwait(false);
                    await AddConnectionsToGroupAsync(parent.UserId, createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);

                    // Add Provider's connections to the Group if he is online.
                    var workshops = await workshopRepository.GetByFilter(w => w.Id == newReceivedAndDeserializedMessage.WorkshopId, "Provider").ConfigureAwait(false);
                    var workshop = workshops.SingleOrDefault();
                    await AddConnectionsToGroupAsync(workshop.Provider.UserId, createdMessageDto.ChatRoomId.ToString(CultureInfo.InvariantCulture) + "WorkshopChat").ConfigureAwait(false);
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

        private async Task<bool> UserHasRigtsForChatRoomAsync(long workshopId, long parentId)
        {
            try
            {
                var userId = Context.User.FindFirst("sub")?.Value;
                var userRole = Context.User.FindFirst("role")?.Value;

                if (userId is null || userRole is null)
                {
                    throw new ArgumentException($"{nameof(userId)} or {nameof(userRole)}");
                }

                bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

                if (userRoleIsProvider)
                {
                    // the user is Provider
                    // and we check if user is owner of workshop
                    return await validationService.UserIsWorkshopOwnerAsync(userId, workshopId).ConfigureAwait(false);
                }
                else
                {
                    // the user is Parent
                    // and we check if user is owner of parent
                    return await validationService.UserIsParentOwnerAsync(userId, parentId).ConfigureAwait(false);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
