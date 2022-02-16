using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller for chat operations between Parent and Provider.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWorkshopController"/> class.
        /// </summary>
        /// <param name="messageService">Service for ChatMessage entities.</param>
        /// <param name="roomService">Service for ChatRoom entities.</param>
        /// <param name="validationService">Service for validation parameters.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="providerAdminService">Service for Provider's admins.</param>
        public ChatWorkshopController(
            IChatMessageWorkshopService messageService,
            IChatRoomWorkshopService roomService,
            IValidationService validationService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<ChatWorkshopController> logger,
            IProviderAdminService providerAdminService)
        {
            this.messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            this.roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
            this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger;
            this.providerAdminService = providerAdminService;
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
        public Task<IActionResult> GetParentsRoomsAsync()
            => this.GetUsersRoomsAsync(parentId => roomService.GetByParentIdAsync(parentId));

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
        public Task<IActionResult> GetProvidersRoomsAsync()
            => this.GetUsersRoomsAsync(providerId => roomService.GetByProviderIdAsync(providerId));

        private async Task<bool> IsParentAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
        {
            var userId = this.GetUserId();

            var result = await validationService.UserIsParentOwnerAsync(userId, chatRoom.ParentId).ConfigureAwait(false);

            if (!result)
            {
                this.LogWarningAboutUsersTryingToGetNotOwnChatRoom(chatRoom.Id, userId);
            }

            return result;
        }

        private async Task<bool> IsProviderAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
        {
            var userId = this.GetUserId();

            var result = await validationService.UserIsWorkshopOwnerAsync(userId, chatRoom.WorkshopId).ConfigureAwait(false);

            if (!result)
            {
                this.LogWarningAboutUsersTryingToGetNotOwnChatRoom(chatRoom.Id, userId);
            }

            return result;
        }

        private string GetUserId()
        {
            var userId = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            return userId;
        }

        private Role GetUserRole()
        {
            var userRoleName = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName, true);

            return userRole;
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
            try
            {
                var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

                if (chatRoom is null)
                {
                    this.LogInfoAboutUsersTryingToGetNotExistingChatRoom(chatRoomId, this.GetUserId());

                    return NoContent();
                }

                var isChatRoomValid = await userHasRights(chatRoom).ConfigureAwait(false);

                if (isChatRoomValid)
                {
                    return Ok(chatRoom);
                }

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.LogWarning(exception.Message);
                var messageForUser = localizer["Can not get some user's claims. Please check your authentication or contact technical support."];
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                var messageForUser = localizer["Server error. Please try again later or contact technical support."];
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }

        private async Task<IActionResult> GetMessagesByRoomIdAsync(Guid chatRoomId, OffsetFilter offsetFilter, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

                if (chatRoom is null)
                {
                    var messageToLog = $"User with userId:{this.GetUserId()} is trying to get messages from not existing chat room: {nameof(chatRoomId)}={chatRoomId}.";
                    logger.LogInformation(messageToLog);

                    return NoContent();
                }

                var isChatRoomValid = await userHasRights(chatRoom).ConfigureAwait(false);

                if (isChatRoomValid)
                {
                    var messages = await messageService.GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(chatRoomId, offsetFilter, this.GetUserRole()).ConfigureAwait(false);

                    if (messages.Any())
                    {
                        return Ok(messages);
                    }

                    return NoContent();
                }

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.LogWarning(exception.Message);
                var messageForUser = localizer["Can not get some user's claims. Please check your authentication or contact technical support."];
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                var messageForUser = localizer["Server error. Please try again later or contact technical support."];
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }

        private async Task<IActionResult> GetUsersRoomsAsync(Func<Guid, Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>> getChatRoomsByRole)
        {
            try
            {
                var userId = this.GetUserId();
                var userRole = this.GetUserRole();
                var providerOrParentId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

                if (providerOrParentId != default)
                {
                    var chatRooms = await getChatRoomsByRole(providerOrParentId).ConfigureAwait(false);

                    if (chatRooms.Any())
                    {
                        return Ok(chatRooms);
                    }
                }
                else if (userRole == Role.Provider)
                {
                    var workshopIds = await providerAdminService.GetRelatedWorkshopIdsForProviderAdmins(userId).ConfigureAwait(false);
                    var chatRooms = await roomService.GetByWorkshopIdsAsync(workshopIds).ConfigureAwait(false);

                    if (chatRooms.Any())
                    {
                        return Ok(chatRooms);
                    }
                }

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.LogWarning(exception.Message);
                var messageForUser = localizer["Can not get some user's claims. Please check your authentication or contact technical support."];
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                var messageForUser = localizer["Server error. Please try again later or contact technical support."];
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }
    }
}
