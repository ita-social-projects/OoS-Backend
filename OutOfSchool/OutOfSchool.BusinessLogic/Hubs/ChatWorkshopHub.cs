using System.Collections.Concurrent;
using System.Security.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Hubs;

[Authorize(Roles = "provider,parent")]
public class ChatWorkshopHub : Hub
{
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
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IBlockedProviderParentService blockedProviderParentService;
    private readonly ICurrentUser currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatWorkshopHub"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="chatMessageService">Service for ChatMessage entities.</param>
    /// <param name="chatRoomService">Service for ChatRoom entities.</param>
    /// <param name="validationService">Service for validation parameters.</param>
    /// <param name="workshopRepository">Repository for workshop entities.</param>
    /// <param name="parentRepository">Repository for parent entities.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="employeeRepository">EmployeeRepository.</param>
    public ChatWorkshopHub(
        ILogger<ChatWorkshopHub> logger,
        IChatMessageWorkshopService chatMessageService,
        IChatRoomWorkshopService chatRoomService,
        IValidationService validationService,
        IWorkshopRepository workshopRepository,
        IParentRepository parentRepository,
        IStringLocalizer<SharedResource> localizer,
        IEmployeeRepository employeeRepository,
        IBlockedProviderParentService blockedProviderParentService,
        ICurrentUser currentUser)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.messageService = chatMessageService ?? throw new ArgumentNullException(nameof(chatMessageService));
        this.roomService = chatRoomService ?? throw new ArgumentNullException(nameof(chatRoomService));
        this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
        this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        this.localizer = localizer;
        this.employeeRepository = employeeRepository;
        this.blockedProviderParentService = blockedProviderParentService;
        this.currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GettingUserProperties.GetUserId(Context.User);
        LogErrorThrowExceptionIfPropertyIsNull(userId, nameof(userId));

        var userRoleName = GettingUserProperties.GetUserRole(Context.User);
        LogErrorThrowExceptionIfPropertyIsNull(userRoleName, nameof(userRoleName));
        
        Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName, true);

        logger.LogDebug($"New Hub-connection established. {nameof(userId)}: {userId}, {nameof(userRoleName)}: {userRoleName}");

        this.AddUsersConnectionIdTracking(userId);

        // Add User to all Groups where he is a member.
        IEnumerable<Guid> usersRoomIds;

        if (userRole == Role.Parent)
        {
            var userRoleId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);
            usersRoomIds = await roomService.GetChatRoomIdsByParentIdAsync(userRoleId).ConfigureAwait(false);
        }
        else
        {
            if (userRole is Role.Employee)
            {
                var employees = await employeeRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false);
                var workshopsIds = employees.SelectMany(admin => admin.ManagedWorkshops, (admin, workshops) => new { workshops }).Select(x => x.workshops.Id);
                usersRoomIds = await roomService.GetChatRoomIdsByWorkshopIdsAsync(workshopsIds).ConfigureAwait(false);
            }
            else
            {
                var userRoleId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);
                usersRoomIds = await roomService.GetChatRoomIdsByProviderIdAsync(userRoleId).ConfigureAwait(false);
            }
        }

        // TODO: add parallel execution (Task.WhenAll(tasks))
        foreach (var id in usersRoomIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString()).ConfigureAwait(false);
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = GettingUserProperties.GetUserId(Context.User);
        LogErrorThrowExceptionIfPropertyIsNull(userId, nameof(userId));

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
            var chatMessageWorkshopCreateDto = JsonSerializerHelper.Deserialize<ChatMessageWorkshopCreateDto>(chatNewMessage);

            var chatRoomExists = await roomService.GetByIdAsync(chatMessageWorkshopCreateDto.ChatRoomId).ConfigureAwait(false) is not null;

            if (!chatRoomExists)
            {
                logger.LogWarning($"{GettingUserProperties.GetUserRole(Context.User)} with UserId:{GettingUserProperties.GetUserId(Context.User)} is trying to send message to not existing chatRoom");

                var messageForUser = localizer["Some of the message parameters were wrong. Please check your message and try again."];
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
                return;
            }

            var userHasRights = await this.UserHasRigtsForChatRoomAsync(chatMessageWorkshopCreateDto.WorkshopId, chatMessageWorkshopCreateDto.ParentId).ConfigureAwait(false);

            if (!userHasRights)
            {
                var messageToLog = $"{GettingUserProperties.GetUserRole(Context.User)} with UserId:{GettingUserProperties.GetUserId(Context.User)} is trying to send message with one of not his own parameters: {nameof(chatMessageWorkshopCreateDto.WorkshopId)} {chatMessageWorkshopCreateDto.WorkshopId}, {nameof(chatMessageWorkshopCreateDto.ParentId)} {chatMessageWorkshopCreateDto.ParentId}";
                logger.LogWarning(messageToLog);

                var messageForUser = localizer["Some of the message parameters were wrong. Please check your message and try again."];
                await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
                return;
            }

            // Save chatMessage in the system.
            Role userRole = (Role)Enum.Parse(typeof(Role), Context.User.FindFirst(IdentityResourceClaimsTypes.Role).Value, true);
            var createdMessageDto = await messageService.CreateAsync(chatMessageWorkshopCreateDto, userRole).ConfigureAwait(false);

            // Add Parent's connections to the Group.
            var parent = await parentRepository.GetById(chatMessageWorkshopCreateDto.ParentId).ConfigureAwait(false);
            await AddConnectionsToGroupAsync(parent.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);

            // Add Provider's connections to the Group.
            var workshops = await workshopRepository.GetByFilter(w => w.Id == chatMessageWorkshopCreateDto.WorkshopId, "Provider").ConfigureAwait(false);
            var workshop = workshops.SingleOrDefault();
            await AddConnectionsToGroupAsync(workshop.Provider.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);

            // Add Provider's deputy connections to the Group.
            var providersDeputies = await employeeRepository.GetByFilter(p => p.ProviderId == workshop.ProviderId).ConfigureAwait(false);
            foreach (var providersDeputy in providersDeputies)
            {
                await AddConnectionsToGroupAsync(providersDeputy.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);
            }

            // Add Provider's admin connections to the Group.
            var providersAdmins = await employeeRepository.GetByFilter(p => p.ManagedWorkshops.Any(w => w.Id == workshop.Id)).ConfigureAwait(false);
            foreach (var providersAdmin in providersAdmins)
            {
                await AddConnectionsToGroupAsync(providersAdmin.UserId, createdMessageDto.ChatRoomId.ToString()).ConfigureAwait(false);
            }

            // Send chatMessage.
            await Clients.Group(createdMessageDto.ChatRoomId.ToString())
                .SendAsync("ReceiveMessageInChatGroup", JsonSerializerHelper.Serialize(createdMessageDto))
                .ConfigureAwait(false);
        }
        catch (AuthenticationException exception)
        {
            logger.LogWarning(exception.Message);
            var messageForUser = localizer["Can not get some user's claims. Please check your authentication or contact technical support."];
            await Clients.Caller.SendAsync("ReceiveMessageInChatGroup", messageForUser).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception.Message);
            var messageForUser = localizer["Server error. Please try again later or contact technical support."];
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

    private async Task<bool> UserHasRigtsForChatRoomAsync(Guid workshopId, Guid parentId)
    {
        var userId = GettingUserProperties.GetUserId(Context.User);
        LogErrorThrowExceptionIfPropertyIsNull(userId, nameof(userId));

        var userRole = GettingUserProperties.GetUserRole(Context.User);
        LogErrorThrowExceptionIfPropertyIsNull(userRole, nameof(userRole));

        bool userRoleIsProvider = currentUser.IsInRole(Role.Provider.ToString());

        bool userRoleIsEmployee = currentUser.IsInRole(Role.Employee.ToString());

        var workshop = await workshopRepository.GetById(workshopId);

        bool isParentBlocked = await blockedProviderParentService.IsBlocked(parentId, workshop.ProviderId);

        if (isParentBlocked)
        {
            return false;
        }

        if (userRoleIsProvider || userRoleIsEmployee)
        {
            var userRoleName = GettingUserProperties.GetUserRole(Context.User);
            LogErrorThrowExceptionIfPropertyIsNull(userRoleName, nameof(userRoleName));

            return await validationService.UserIsWorkshopOwnerAsync(userId, workshopId);
        }

        return await validationService.UserIsParentOwnerAsync(userId, parentId);
    }

    private void LogErrorThrowExceptionIfPropertyIsNull(string claim, string nameofClaim)
    {
        if (claim is null)
        {
            var errorText = "Can not get user's claim {0} from HttpContext.";
            logger.LogError(string.Format(CultureInfo.InvariantCulture, errorText, nameofClaim));
            throw new AuthenticationException(localizer[errorText, nameofClaim]);
        }
    }
}