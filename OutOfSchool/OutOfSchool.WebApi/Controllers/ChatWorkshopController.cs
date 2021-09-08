using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller for chat operations between Parent and Provider.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class ChatWorkshopController : ControllerBase
    {
        private readonly IChatMessageWorkshopService messageService;
        private readonly IChatRoomWorkshopService roomService;
        private readonly IValidationService validationService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWorkshopController"/> class.
        /// </summary>
        /// <param name="messageService">Service for ChatMessage entities.</param>
        /// <param name="roomService">Service for ChatRoom entities.</param>
        /// <param name="validationService">Service for validation parameters.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger">Logger.</param>
        public ChatWorkshopController(
            IChatMessageWorkshopService messageService,
            IChatRoomWorkshopService roomService,
            IValidationService validationService,
            IStringLocalizer<SharedResource> localizer,
            ILogger logger)
        {
            this.messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            this.roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
            this.validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger;
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
        public Task<IActionResult> GetRoomForParentByRoomIdAsync([Range(1, long.MaxValue)] long id)
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
        public Task<IActionResult> GetRoomForProviderByRoomIdAsync([Range(1, long.MaxValue)] long id)
            => this.GetRoomByIdAsync(id, this.IsProviderAChatRoomParticipantAsync);

        /// <summary>
        /// Get a portion of chat messages for specified parent's chat room.
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
        public Task<IActionResult> GetMessagesForParentByRoomIdAsync([Range(1, long.MaxValue)] long id, [FromQuery] OffsetFilter offsetFilter)
            => this.GetMessagesByRoomIdAsync(id, offsetFilter, this.IsParentAChatRoomParticipantAsync);

        /// <summary>
        /// Get a portion of chat messages for specified provider's chat room.
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
        public Task<IActionResult> GetMessagesForProviderByRoomIdAsync([Range(1, long.MaxValue)] long id, [FromQuery] OffsetFilter offsetFilter)
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

        /// <summary>
        /// Set current date and time for all not read chat messages for parent in the specified chat room.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>Number of successfully updated items.</returns>
        [HttpPatch("parent/chatrooms/{id}/read")]
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> SetReadDatetimeInMessagesForParentAsync([Range(1, long.MaxValue)] long id)
            => this.SetReadDatetimeInMessagesForCurrentUserAsync(id, this.IsParentAChatRoomParticipantAsync);

        /// <summary>
        /// Set current date and time for all not read chat messages for provider in the specified chat room.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>Number of successfully updated items.</returns>
        [HttpPatch("provider/chatrooms/{id}/read")]
        [Authorize(Roles = "provider")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> SetReadDatetimeInMessagesForProviderAsync([Range(1, long.MaxValue)] long id)
            => this.SetReadDatetimeInMessagesForCurrentUserAsync(id, this.IsProviderAChatRoomParticipantAsync);

        private Task<bool> IsParentAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
        {
            var userId = this.GetUserId();

            return validationService.UserIsParentOwnerAsync(userId, chatRoom.ParentId);
        }

        private Task<bool> IsProviderAChatRoomParticipantAsync(ChatRoomWorkshopDto chatRoom)
        {
            var userId = this.GetUserId();

            return validationService.UserIsWorkshopOwnerAsync(userId, chatRoom.WorkshopId);
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

        private void LogWarningAboutUsersTryingToGetNotOwnChatRoomInformation(long chatRoomId)
        {
            var messageToLog = $"{this.GetUserRole()} with UserId:{this.GetUserId()} is trying to get not his own chat room: {nameof(chatRoomId)}={chatRoomId}.";
            logger.Warning(messageToLog);
        }

        private async Task<IActionResult> GetRoomByIdAsync(long chatRoomId, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

                var isChatRoomValid = chatRoom != null && await userHasRights(chatRoom).ConfigureAwait(false);

                if (isChatRoomValid)
                {
                    return Ok(chatRoom);
                }

                this.LogWarningAboutUsersTryingToGetNotOwnChatRoomInformation(chatRoomId);

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.Warning(exception.Message);
                var messageForUser = "Can not get some user's claims. Please check your authentication or contact technical support.";
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message);
                var messageForUser = "Server error. Please try again later or contact technical support.";
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }

        private async Task<IActionResult> GetMessagesByRoomIdAsync(long chatRoomId, OffsetFilter offsetFilter, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

                var isChatRoomValid = chatRoom != null && await userHasRights(chatRoom).ConfigureAwait(false);

                if (isChatRoomValid)
                {
                    var messages = await messageService.GetMessagesForChatRoomAsync(chatRoomId, offsetFilter).ConfigureAwait(false);

                    if (messages.Any())
                    {
                        return Ok(messages);
                    }

                    return NoContent();
                }

                this.LogWarningAboutUsersTryingToGetNotOwnChatRoomInformation(chatRoomId);

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.Warning(exception.Message);
                var messageForUser = "Can not get some user's claims. Please check your authentication or contact technical support.";
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message);
                var messageForUser = "Server error. Please try again later or contact technical support.";
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }

        private async Task<IActionResult> GetUsersRoomsAsync(Func<long, Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>>> getChatRoomsByRole)
        {
            try
            {
                var providerOrParentId = await validationService.GetParentOrProviderIdByUserRoleAsync(this.GetUserId(), this.GetUserRole()).ConfigureAwait(false);

                if (providerOrParentId != default)
                {
                    var chatRooms = await getChatRoomsByRole(providerOrParentId).ConfigureAwait(false);

                    if (chatRooms.Any())
                    {
                        return Ok(chatRooms);
                    }
                }

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.Warning(exception.Message);
                var messageForUser = "Can not get some user's claims. Please check your authentication or contact technical support.";
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message);
                var messageForUser = "Server error. Please try again later or contact technical support.";
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }

        private async Task<IActionResult> SetReadDatetimeInMessagesForCurrentUserAsync(long chatRoomId, Func<ChatRoomWorkshopDto, Task<bool>> userHasRights)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

                var isChatRoomValid = chatRoom != null && await userHasRights(chatRoom).ConfigureAwait(false);

                if (isChatRoomValid)
                {
                    var numberOfUpdatedMessages = await messageService.SetReadDatetimeInAllMessagesForUserInChatRoomAsync(chatRoomId, this.GetUserRole()).ConfigureAwait(false);

                    return Ok(numberOfUpdatedMessages);
                }

                this.LogWarningAboutUsersTryingToGetNotOwnChatRoomInformation(chatRoomId);

                return NoContent();
            }
            catch (AuthenticationException exception)
            {
                logger.Warning(exception.Message);
                var messageForUser = "Can not get some user's claims. Please check your authentication or contact technical support.";
                return BadRequest(messageForUser);
            }
            catch (Exception exception)
            {
                logger.Error(exception.Message);
                var messageForUser = "Server error. Please try again later or contact technical support.";
                return new ObjectResult(messageForUser) { StatusCode = 500 };
            }
        }
    }
}
