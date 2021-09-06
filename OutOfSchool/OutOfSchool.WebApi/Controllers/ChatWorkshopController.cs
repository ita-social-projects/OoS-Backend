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
        /// Get a chat room with information about Parent and Workshop.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>User's chat room that was found.</returns>
        [HttpGet("chatrooms/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomWorkshopDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoomByIdAsync([Range(1, long.MaxValue)] long id)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(id).ConfigureAwait(false);

                if (!(chatRoom is null) &&
                    await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false))
                {
                    return Ok(chatRoom);
                }

                var messageToLog = $"{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)} with UserId:{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)} is trying to get not his own chat room: {nameof(id)} {id}.";
                logger.Warning(messageToLog);

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

        /// <summary>
        /// Get a portion of chat messages for specified ChatRoom.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <param name="offsetFilter">Filter to get specified portion of messages in the chat room.</param>
        /// <returns>User's chat room's messages that were found.</returns>
        [HttpGet("chatrooms/{id}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatMessageWorkshopDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMessagesByRoomIdAsync([Range(1, long.MaxValue)] long id, [FromQuery] OffsetFilter offsetFilter)
        {
            try
            {
                var chatRoom = await roomService.GetByIdAsync(id).ConfigureAwait(false);

                if (!(chatRoom is null)
                    && await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false))
                {
                    var messages = await messageService.GetMessagesForChatRoomAsync(id, offsetFilter).ConfigureAwait(false);

                    if (messages.Any())
                    {
                        return Ok(messages);
                    }

                    return NoContent();
                }

                var messageToLog = $"{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)} with UserId:{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)} is trying to get not his own chat room: {nameof(id)} {id}.";
                logger.Warning(messageToLog);

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

        /// <summary>
        /// Get a list of ChatRooms for current user.
        /// </summary>
        /// <returns>List of ChatRooms with last message and number of not read messages.</returns>
        [HttpGet("chatrooms")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersRoomsAsync()
        {
            try
            {
                var userId = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                    ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

                var userRoleName = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                    ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

                Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName);

                var userRoleId = await validationService.GetParentOrProviderIdByUserRoleAsync(userId, userRole).ConfigureAwait(false);

                var chatRooms = (userRole == Role.Parent)
                    ? await roomService.GetByParentIdAsync(userRoleId).ConfigureAwait(false)
                    : await roomService.GetByProviderIdAsync(userRoleId).ConfigureAwait(false);

                if (chatRooms.Any())
                {
                    return Ok(chatRooms);
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

        // TODO: consider to change method logic, if user gets only part of unread messages

        /// <summary>
        /// Set status IsRead on true for each user's ChatMessage in the ChatRoom.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>Number of successfully updated items.</returns>
        [HttpPut("chatrooms/{id}/read")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessagesStatusAsync([Range(1, long.MaxValue)] long id)
        {
            try
            {
                var room = await roomService.GetByIdAsync(id).ConfigureAwait(false);

                if (room is null ||
                    !await this.UserHasRigtsForChatRoomAsync(room.WorkshopId, room.ParentId).ConfigureAwait(false))
                {
                    var messageToLog = $"{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)} with UserId:{HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)} is trying to get not his own chat room: {nameof(id)} {id}.";
                    logger.Warning(messageToLog);

                    return NoContent();
                }

                var userRoleName = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                    ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

                Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName);

                var numberOfUpdatedMessages = await messageService.UpdateIsReadByCurrentUserInChatRoomAsync(id, userRole).ConfigureAwait(false);

                if (numberOfUpdatedMessages > 0)
                {
                    return Ok(numberOfUpdatedMessages);
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

        private Task<bool> UserHasRigtsForChatRoomAsync(long workshopId, long parentId)
        {
            var userId = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            var userRole = HttpContext.User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (userRoleIsProvider)
            {
                return validationService.UserIsWorkshopOwnerAsync(userId, workshopId);
            }

            return validationService.UserIsParentOwnerAsync(userId, parentId);
        }
    }
}
