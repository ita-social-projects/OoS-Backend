using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller for chat operations between Parent and Provider.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "provider,parent,ministryadmin")]
public class ChatWorkshopController : ControllerBase
{
    // TODO: define the algorithm of logging information and warnings  in the solution
    // TODO: add localization to response
    private readonly IChatMessageWorkshopService messageService;
    private readonly IChatRoomWorkshopService roomService;
    private readonly IValidationService validationService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ILogger<ChatWorkshopController> logger;
    private readonly IProviderAdminService providerAdminService;
    private readonly IMinistryAdminService ministryAdminService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatWorkshopController"/> class.
    /// </summary>
    /// <param name="messageService">Service for ChatMessage entities.</param>
    /// <param name="roomService">Service for ChatRoom entities.</param>
    /// <param name="validationService">Service for validation parameters.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="providerAdminService">Service for Provider's admins.</param>
    /// <param name="ministryAdminService">Service for Ministry admins.</param>
    public ChatWorkshopController(
        IChatMessageWorkshopService messageService,
        IChatRoomWorkshopService roomService,
        IValidationService validationService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ChatWorkshopController> logger,
        IProviderAdminService providerAdminService,
        IMinistryAdminService ministryAdminService)
    {
        this.messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        this.roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.providerAdminService = providerAdminService ?? throw new ArgumentNullException(nameof(providerAdminService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
    }

    /// <summary>
    /// Get parent's chat room with information about Parent and Workshop.
    /// </summary>
    /// <param name="id">ChatRoom's Id.</param>
    /// <returns>User's chat room that was found.</returns>
    [HttpGet("parent/chatrooms/{id}")]
    [Authorize(Roles = "parent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomWorkshopDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetRoomForParentByRoomIdAsync(Guid id)
        => this.GetRoomByIdAsync(id, this.IsParentAChatRoomParticipantAsync);

    /// <summary>
    /// Get provider's chat room with information about Parent and Workshop.
    /// </summary>
    /// <param name="id">ChatRoom's Id.</param>
    /// <returns>User's chat room that was found.</returns>
    [HttpGet("provider/chatrooms/{id}")]
    [Authorize(Roles = "provider")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomWorkshopDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetRoomForProviderByRoomIdAsync(Guid id)
        => this.GetRoomByIdAsync(id, this.IsProviderAChatRoomParticipantAsync);

    /// <summary>
    /// Get a portion of chat messages for specified parent's chat room.
    /// Set read current date and time in UTC format in messages that are not read by the parent.
    /// </summary>
    /// <param name="id">ChatRoom's Id.</param>
    /// <param name="offsetFilter">Filter to get specified portion of messages in the chat room.</param>
    /// <returns>User's chat room's messages that were found.</returns>
    [HttpGet("parent/chatrooms/{id}/messages")]
    [Authorize(Roles = "parent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatMessageWorkshopDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetMessagesForParentByRoomIdAsync(Guid id, [FromQuery] OffsetFilter offsetFilter)
        => this.GetMessagesByRoomIdAsync(id, offsetFilter, this.IsParentAChatRoomParticipantAsync);

    /// <summary>
    /// Get a portion of chat messages for specified provider's chat room.
    /// Set read current date and time in UTC format in messages that are not read by the provider.
    /// </summary>
    /// <param name="id">ChatRoom's Id.</param>
    /// <param name="offsetFilter">Filter to get specified portion of messages in the chat room.</param>
    /// <returns>User's chat room's messages that were found.</returns>
    [HttpGet("provider/chatrooms/{id}/messages")]
    [Authorize(Roles = "provider")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatMessageWorkshopDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetMessagesForProviderByRoomIdAsync(Guid id, [FromQuery] OffsetFilter offsetFilter)
        => this.GetMessagesByRoomIdAsync(id, offsetFilter, this.IsProviderAChatRoomParticipantAsync);

    /// <summary>
    /// Gets a portion of chat messages for Ministry Admin if it has related provider participants by specified chat room id.
    /// </summary>
    /// <param name="id">ChatRoom's Id.</param>
    /// <param name="offsetFilter">Filter to get specified portion of messages in the chat room.</param>
    /// <returns>User's chat room's messages that were found.</returns>
    [HttpGet("ministryadmin/chatrooms/{id}/messages")]
    [Authorize(Roles = "ministryadmin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatMessageWorkshopDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetMessagesForMinistryAdminByRoomIdAsync(Guid id, [FromQuery] OffsetFilter offsetFilter)
        => this.GetMessagesByRoomIdAsync(id, offsetFilter, IsMinistryAdminAbleToBeSeeChatRoomMessagesAsync);

    /// <summary>
    /// Get a list of chat rooms for current parent.
    /// </summary>
    /// <returns>List of ChatRooms with last message and number of not read messages.</returns>
    [HttpGet("parent/chatrooms")]
    [Authorize(Roles = "parent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetParentsRoomsAsync([FromQuery] ChatWorkshopFilter filter = null)
        => this.GetUsersRoomsAsync(parentId => roomService.GetByParentIdAsync(parentId), filter);

    /// <summary>
    /// Get a list of chat rooms for current provider.
    /// </summary>
    /// <returns>List of ChatRooms with last message and number of not read messages.</returns>
    [HttpGet("provider/chatrooms")]
    [Authorize(Roles = "provider")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetProvidersRoomsAsync([FromQuery] ChatWorkshopFilter filter = null)
        => this.GetUsersRoomsAsync(providerId => roomService.GetByProviderIdAsync(providerId), filter);

    /// <summary>
    /// Get a chat room for current parent and workshopId.
    /// </summary>
    /// <param name="workshopId">WorkShop's Id.</param>
    /// <returns>ChatRoom that was found.</returns>
    [HttpGet("parent/chatroomforworkshop/{workshopId}")]
    [Authorize(Roles = "parent")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomWorkshopDtoWithLastMessage))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetParentsRoomByWorkshopAsync(Guid workshopId)
        => GetParentRoomByWorkshopIdAsync(workshopId);

    /// <summary>
    /// Get a chat room for current provider and parentId.
    /// </summary>
    /// <param name="parentId">ChatRoom's Id.</param>
    /// <returns>ChatRoom that was found.</returns>
    [HttpGet("provider/chatroomsforparent/{parentId}")]
    [Authorize(Roles = "provider")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetProvidersRoomsByParentAsync(Guid parentId)
        => GetProvidersRoomsByParentIdAsync(parentId);

    /// <summary>
    /// Get a list of chat rooms for the ministry admin by specific provider id.
    /// </summary>
    /// <param name="providerId">Provider Id.</param>
    /// <returns>List of ChatRooms with last message and number of not read messages.</returns>
    [HttpGet("ministryadmin/chatrooms")]
    [Authorize(Roles = "ministryadmin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRoomsForMinistryAdminByProviderIdAsync([FromQuery] Guid providerId)
    {
        async Task<IActionResult> Operation()
        {
            var userId = GettingUserProperties.GetUserId(HttpContext);

            var isProviderSubordinate = await ministryAdminService.IsProviderSubordinateAsync(userId, providerId);
            if (!isProviderSubordinate)
            {
                return Forbid();
            }

            var chatRooms = await roomService.GetByProviderIdAsync(providerId);
            return chatRooms.Any() ? Ok(chatRooms) : NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private async Task<bool> IsParentAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);

        var result = await validationService.UserIsParentOwnerAsync(userId, chatRoom.ParentId).ConfigureAwait(false);

        if (!result)
        {
            this.LogWarningAboutUsersTryingToGetNotOwnChatRoom(chatRoom.Id, userId);
        }

        return result;
    }

    private async Task<bool> IsProviderAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);
        var userSubrole = GettingUserProperties.GetUserSubrole(HttpContext);

        var result = await validationService.UserIsWorkshopOwnerAsync(userId, chatRoom.WorkshopId, userSubrole).ConfigureAwait(false);

        if (!result)
        {
            this.LogWarningAboutUsersTryingToGetNotOwnChatRoom(chatRoom.Id, userId);
        }

        return result;
    }

    private async Task<bool> IsMinistryAdminAbleToBeSeeChatRoomMessagesAsync(ChatRoomWorkshopDto chatRoom)
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);

        return await ministryAdminService.IsProviderSubordinateAsync(
            userId,
            chatRoom.Workshop?.ProviderId ?? throw new InvalidOperationException($"Unable to retrieve workshop data from chatRoom with id = {chatRoom.Id}"));
    }

    private void LogWarningAboutUsersTryingToGetNotOwnChatRoom(Guid chatRoomId, string userId)
    {
        var messageToLog = $"User with {nameof(userId)}:{userId} is trying to get not his own chat room: {nameof(chatRoomId)}={chatRoomId}.";
        logger.LogWarning(messageToLog);
    }

    private void LogInfoAboutUsersTryingToGetNotExistingChatRoom(Guid chatRoomId, string userId)
    {
        var messageToLog = $"User with {nameof(userId)}:{userId} is trying to get not existing chat room: {nameof(chatRoomId)}={chatRoomId}.";
        logger.LogInformation(messageToLog);
    }

    private async Task<IActionResult> GetRoomByIdAsync(Guid chatRoomId, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
    {
        async Task<IActionResult> Operation()
        {
            var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

            if (chatRoom is null)
            {
                LogInfoAboutUsersTryingToGetNotExistingChatRoom(chatRoomId, GettingUserProperties.GetUserId(HttpContext));

                return NoContent();
            }

            var isChatRoomValid = await userHasRights(chatRoom).ConfigureAwait(false);

            return isChatRoomValid ? Ok(chatRoom) : NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private async Task<IActionResult> GetMessagesByRoomIdAsync(Guid chatRoomId, OffsetFilter offsetFilter, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
    {
        async Task<IActionResult> Operation()
        {
            var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

            if (chatRoom is null)
            {
                var messageToLog = $"User with userId:{GettingUserProperties.GetUserId(HttpContext)} is trying to get messages from not existing chat room: {nameof(chatRoomId)}={chatRoomId}.";
                logger.LogInformation(messageToLog);

                return NoContent();
            }

            var isChatRoomValid = await userHasRights(chatRoom).ConfigureAwait(false);

            if (isChatRoomValid)
            {
                var messages = await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(chatRoomId, offsetFilter, GettingUserProperties.GetUserRole(HttpContext)).ConfigureAwait(false);

                return messages.Any() ? Ok(messages) : NoContent();
            }

            return NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private async Task<IActionResult> GetParentRoomByWorkshopIdAsync(Guid workshopId)
    {
        async Task<IActionResult> Operation()
        {
            var userId = GettingUserProperties.GetUserId(HttpContext);
            var userRole = Role.Parent;

            var parentId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

            if (parentId != Guid.Empty)
            {
                var chatRoom = await roomService.GetByParentIdWorkshopIdAsync(parentId, workshopId).ConfigureAwait(false);

                if (chatRoom is not null)
                {
                    return Ok(chatRoom);
                }
            }

            return NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private async Task<IActionResult> GetProvidersRoomsByParentIdAsync(Guid parentId)
    {
        async Task<IActionResult> Operation()
        {
            var userId = GettingUserProperties.GetUserId(HttpContext);
            var userRole = Role.Provider;

            var providerId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

            if (providerId != Guid.Empty)
            {
                var chatRooms = await roomService.GetByParentIdProviderIdAsync(parentId, providerId).ConfigureAwait(false);

                if (chatRooms.Any())
                {
                    return Ok(chatRooms);
                }
            }

            return NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private async Task<IActionResult> GetUsersRoomsAsync(Func<Guid, Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>> getChatRoomsByRole, ChatWorkshopFilter filter)
    {
        async Task<IActionResult> Operation()
        {
            var userId = GettingUserProperties.GetUserId(HttpContext);
            var userRole = GettingUserProperties.GetUserRole(HttpContext);
            var userSubrole = GettingUserProperties.GetUserSubrole(HttpContext);

            if (userSubrole == Subrole.ProviderAdmin)
            {
                var workshopIds = await providerAdminService.GetRelatedWorkshopIdsForProviderAdmins(userId).ConfigureAwait(false);
                filter.WorkshopIds = workshopIds;
                var chatRooms = await roomService.GetChatRoomByFilter(filter, default).ConfigureAwait(false);

                return chatRooms.Any() ? Ok(chatRooms) : NoContent();
            }

            var providerOrParentId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

            if (providerOrParentId != default)
            {
                IEnumerable<ChatRoomWorkshopDtoWithLastMessage> chatRooms = null;
                if (IsFilterEmpty(filter))
                {
                    chatRooms = await getChatRoomsByRole(providerOrParentId).ConfigureAwait(false);
                }
                else
                {
                    chatRooms = await roomService.GetChatRoomByFilter(filter, providerOrParentId).ConfigureAwait(false);
                }
                if (chatRooms.Any())
                {
                    return Ok(chatRooms);
                }
            }

            return NoContent();
        }

        return await HandleOperationAsync(Operation);
    }

    private bool IsFilterEmpty(ChatWorkshopFilter filter)
    {
        filter ??= new ChatWorkshopFilter();
        filter.WorkshopIds ??= new List<Guid>();
        return filter.WorkshopIds.Count() == 0 && string.IsNullOrEmpty(filter.SearchText) && filter.From == 0;
    }

    private async Task<IActionResult> HandleOperationAsync(Func<Task<IActionResult>> operation)
    {
        try
        {
            return await operation();
        }
        catch (AuthenticationException exception)
        {
            logger.LogError(exception, "Unable to authenticate user");
            var messageForUser = localizer["Can not get some user's claims. Please check your authentication or contact technical support."];
            return BadRequest(messageForUser);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Server error. Unable to access the hub");
            var messageForUser = localizer["Server error. Please try again later or contact technical support."];
            return new ObjectResult(messageForUser) { StatusCode = 500 };
        }
    }
}