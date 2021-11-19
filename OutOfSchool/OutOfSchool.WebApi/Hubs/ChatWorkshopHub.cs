using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class ChatWorkshopHub : Hub
    {
        // TODO: add/change proper logging of all exceptions
        // TODO: split method SendMessageToOthersInGroupAsync into two separate for parent and provider

        // This collection tracks users with their connections.
        private static readonly ConcurrentDictionary<string, HashSet<string>> UsersConnections
            = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ILogger<ChatWorkshopHub> logger;
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
        public ChatWorkshopHub(
            ILogger<ChatWorkshopHub> logger,
            IChatMessageWorkshopService chatMessageService,
            IChatRoomWorkshopService chatRoomService,
            IValidationService validationService,
            IWorkshopRepository workshopRepository,
            IParentRepository parentRepository)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.messageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
            this.roomService = chatRoomService ?? throw new ArgumentNullException(nameof(chatRoomService));
            this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
            this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            var userRoleName = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Role)} from HttpContext.");

            Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName, true);

            logger.LogDebug($"New Hub-connection established. {nameof(userId)}: {userId}, {nameof(userRoleName)}: {userRoleName}");

            this.AddUsersConnectionIdTracking(userId);

            // Add User to all Groups where he is a member.
            IEnumerable<Guid> usersRoomIds;

            var userRoleId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

            usersRoomIds = (userRole == Role.Parent)
                ? await roomService.GetChatRoomIdsByParentIdAsync(userRoleId).ConfigureAwait(false)
                : await roomService.GetChatRoomIdsByProviderIdAsync(userRoleId).ConfigureAwait(false);

            // TODO: add parallel execution (Task.WhenAll(tasks))
            foreach (var id in usersRoomIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString()).ConfigureAwait(false);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            logger.LogDebug($"UserId: {userId} connection:{Context.ConnectionId} disconnected.");

            this.RemoveUsersConnectionIdTracking(userId);
        }

        /// <summary>
        /// Creates a <see cref="ChatMessageWorkshopDto"/>, saves it to the DataBase and sends message to Others in Group.
        /// </summary>
        /// <param name="chatNewMessage">Entity (string format) that contains text of message, receiver and workshop info.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task SendMessageToOthersInGroupAsync(string chatNewMessage)
        {
            logger.LogDebug($"{nameof(SendMessageToOthersInGroupAsync)}.Invoked.");

            try
            {
                // Deserialize from string to Object
                var chatMessageWorkshopCreateDto = JsonConvert.DeserializeObject<ChatMessageWorkshopCreateDto>(chatNewMessage);

                var userHasRights = await this.UserHasRigtsForChatRoomAsync(chatMessageWorkshopCreateDto.WorkshopId, chatMessageWorkshopCreateDto.ParentId).ConfigureAwait(false);

                if (!userHasRights)
                {
                    var messageToLog = $"{Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)} with UserId:{Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)} is trying to send message with one of not his own parameters: {nameof(chatMessageWorkshopCreateDto.WorkshopId)} {chatMessageWorkshopCreateDto.WorkshopId}, {nameof(chatMessageWorkshopCreateDto.ParentId)} {chatMessageWorkshopCreateDto.ParentId}";
                    logger.LogWarning(messageToLog);

                    var messageForUser = "Some of the message parameters were wrong. Please check your message and try again.";
                    await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
                    return;
                }

                // Save chatMessage in the system.
                Role userRole = (Role)Enum.Parse(typeof(Role), Context.User.FindFirst("role").Value, true);
                var createdMessageDto = await messageService.CreateAsync(chatMessageWorkshopCreateDto, userRole).ConfigureAwait(false);

                // Add Parent's connections to the Group.
                var parent = await parentRepository.GetById(chatMessageWorkshopCreateDto.ParentId).ConfigureAwait(false);
                await AddConnectionsToGroupAsync(parent.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);

                // Add Provider's connections to the Group.
                var workshops = await workshopRepository.GetByFilter(w => w.Id == chatMessageWorkshopCreateDto.WorkshopId, "Provider").ConfigureAwait(false);
                var workshop = workshops.SingleOrDefault();
                await AddConnectionsToGroupAsync(workshop.Provider.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);

                // Send chatMessage.
                await Clients.OthersInGroup(createdMessageDto.ChatRoomId.ToString())
                    .SendAsync("ReceiveMessageInChatGroup", JsonConvert.SerializeObject(createdMessageDto))
                    .ConfigureAwait(false);
            }
            catch (AuthenticationException exception)
            {
                logger.LogWarning(exception.Message);
                var messageForUser = "Can not get some user's claims. Please check your authentication or contact technical support.";
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                var messageForUser = "Server error. Please try again later or contact technical support.";
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
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

        private Task<bool> UserHasRigtsForChatRoomAsync(Guid workshopId, Guid parentId)
        {
            var userId = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            var userRole = Context.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Role)} from HttpContext.");

            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (userRoleIsProvider)
            {
                return validationService.UserIsWorkshopOwnerAsync(userId, workshopId);
            }

            return validationService.UserIsParentOwnerAsync(userId, parentId);
        }
    }
}
