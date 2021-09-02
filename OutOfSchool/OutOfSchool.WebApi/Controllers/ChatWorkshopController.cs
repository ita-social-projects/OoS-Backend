using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller for Chat operations.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "provider,parent")]
    public class ChatWorkshopController : ControllerBase
    {
        // TODO: consider to return 404 instead of 403
        // TODO: remove validation to service
        private readonly IChatMessageWorkshopService messageService;
        private readonly IChatRoomWorkshopService roomService;
        private readonly IValidationService validationService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatWorkshopController"/> class.
        /// </summary>
        /// <param name="messageService">Service for ChatMessage entities.</param>
        /// <param name="roomService">Service for ChatRoom entities.</param>
        /// <param name="validationService">Service for validation parameters.</param>
        /// <param name="localizer">Localizer.</param>
        public ChatWorkshopController(IChatMessageWorkshopService messageService, IChatRoomWorkshopService roomService, IValidationService validationService, IStringLocalizer<SharedResource> localizer)
        {
            // TODO: add check for null
            this.messageService = messageService;
            this.roomService = roomService;
            this.validationService = validationService;
            this.localizer = localizer;
        }

        /// <summary>
        /// Create new ChatMessage.
        /// </summary>
        /// <param name="chatMessageWorkshopCreateDto">Entity that contains text of message, receiver and workshop info.</param>
        /// <returns>Created <see cref="ChatMessageWorkshopDto"/>.</returns>
        [Obsolete("This method is for testing purposes.")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ChatMessageWorkshopDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMessageAsync(ChatMessageWorkshopCreateDto chatMessageWorkshopCreateDto)
        {
            // TODO: delete exception, add BadRequest
            if (chatMessageWorkshopCreateDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageWorkshopCreateDto)}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userRole = HttpContext.User.FindFirst("role")?.Value;

            if (userRole is null)
            {
                throw new ArgumentException($"{nameof(userRole)}");
            }

            var userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            // create new dto object that will be saved to the database
            var chatMessageDtoThatWillBeSaved = new ChatMessageWorkshopDto()
            {
                SenderRoleIsProvider = userRoleIsProvider,
                Text = chatMessageWorkshopCreateDto.Text,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ReadDateTime = null,
                ChatRoomId = 0,
            };

            var userHasRights = await this.UserHasRigtsForChatRoomAsync(chatMessageWorkshopCreateDto.WorkshopId, chatMessageWorkshopCreateDto.ParentId).ConfigureAwait(false);

            if (userHasRights)
            {
                // set the unique ChatRoomId property according to WorkshopId and ParentId
                // TODO: delete check for unique chat room as we don't need it here
                var existingRoom = await roomService.GetUniqueChatRoomAsync(chatMessageWorkshopCreateDto.WorkshopId, chatMessageWorkshopCreateDto.ParentId)
                    .ConfigureAwait(false);

                if (existingRoom is null)
                {
                    var newChatRoomDto = await roomService.CreateOrReturnExistingAsync(chatMessageWorkshopCreateDto.WorkshopId, chatMessageWorkshopCreateDto.ParentId)
                        .ConfigureAwait(false);
                    chatMessageDtoThatWillBeSaved.ChatRoomId = newChatRoomDto.Id;
                }
                else
                {
                    chatMessageDtoThatWillBeSaved.ChatRoomId = existingRoom.Id;
                }

                var createdMessageDto = await messageService.CreateAsync(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);

                return new CreatedResult(string.Empty, createdMessageDto);
            }
            else
            {
                string message = "Some of the message parameters were wrong. User has no rights for this chat room or sender role is set invalid.";
                return StatusCode(403, message);
            }
        }

        /// <summary>
        /// Get ChatRoom without ChatMessages by ChatRoomId.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>ChatRoom that was found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomWorkshopDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoomByIdAsync(long id)
        {
            this.ValidateId(id, localizer);

            var chatRoom = await roomService.GetByIdAsync(id).ConfigureAwait(false);

            if (chatRoom is null)
            {
                return NoContent();
            }

            if (await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false))
            {
                return Ok(chatRoom);
            }
            else
            {
                return StatusCode(403, "Forbidden to get a chat room of another users.");
            }
        }

        /// <summary>
        /// Get a portion of chat messages for specified ChatRoom.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <param name="offsetFilter">Filter to get specified portion of messages in the chat room.</param>
        /// <returns>ChatRoom that was found.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatMessageWorkshopDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMessagesByRoomIdAsync(long id, [FromQuery] OffsetFilter offsetFilter)
        {
            this.ValidateId(id, localizer);

            var chatRoom = await roomService.GetByIdAsync(id).ConfigureAwait(false);

            if (chatRoom is null)
            {
                return NoContent();
            }
            else
            {
                if (await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false))
                {
                    var messages = await messageService.GetMessagesForChatRoomAsync(id, offsetFilter).ConfigureAwait(false);

                    if (messages.Any())
                    {
                        return Ok(messages);
                    }
                    else
                    {
                        return NoContent();
                    }
                }
                else
                {
                    return StatusCode(403, "Forbidden to get a chat room of another users.");
                }
            }
        }

        /// <summary>
        /// Get a list of ChatRooms for current user.
        /// </summary>
        /// <returns>List of ChatRooms with last message and number of not read messages.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWorkshopDtoWithLastMessage>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersRoomsAsync()
        {
            var userId = HttpContext.User.FindFirst("sub")?.Value;
            var userRole = HttpContext.User.FindFirst("role")?.Value;

            if (userId is null || userRole is null)
            {
                throw new ArgumentException($"{nameof(userId)} or {nameof(userRole)}");
            }

            IEnumerable<ChatRoomWorkshopDtoWithLastMessage> chatRooms;

            // TODO: simplify this like in Hub
            if (string.Equals(userRole, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var providerId = await validationService.GetEntityIdAccordingToUserRoleAsync(userId, userRole).ConfigureAwait(false);
                chatRooms = await roomService.GetByProviderIdAsync(providerId).ConfigureAwait(false);
            }
            else
            {
                var parentId = await validationService.GetEntityIdAccordingToUserRoleAsync(userId, userRole).ConfigureAwait(false);
                chatRooms = await roomService.GetByParentIdAsync(parentId).ConfigureAwait(false);
            }

            if (!chatRooms.Any())
            {
                return NoContent();
            }

            return Ok(chatRooms);
        }

        // TODO: consider to change method logic, if user gets only part of unread messages

        /// <summary>
        /// Set status IsRead on true for each user's ChatMessage in the ChatRoom.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's Id.</param>
        /// <returns>Number of successfully updated items.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessagesStatusAsync(long chatRoomId)
        {
            this.ValidateId(chatRoomId, localizer);

            var room = await roomService.GetByIdAsync(chatRoomId).ConfigureAwait(false);

            if (room is null)
            {
                return NoContent();
            }

            if (!await this.UserHasRigtsForChatRoomAsync(room.WorkshopId, room.ParentId).ConfigureAwait(false))
            {
                return BadRequest($"User is not a participant of the chat:{chatRoomId}");
            }

            var userRole = HttpContext.User.FindFirst("role")?.Value;
            if (userRole is null)
            {
                throw new ArgumentException($"{nameof(userRole)}");
            }

            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            var numberOfUpdatedMessages = await messageService.UpdateIsReadByCurrentUserInChatRoomAsync(chatRoomId, userRoleIsProvider).ConfigureAwait(false);

            if (numberOfUpdatedMessages > 0)
            {
                return Ok(numberOfUpdatedMessages);
            }
            else
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Update ChatMessage info.
        /// </summary>
        /// <param name="updatedChatMessage">Entity that will be updated.</param>
        /// <returns>Successfully updated item.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatMessageWorkshopDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessageAsync(ChatMessageWorkshopUpdateDto updatedChatMessage)
        {
            if (updatedChatMessage is null)
            {
                throw new ArgumentNullException($"{nameof(updatedChatMessage)}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var oldChatMessage = await messageService.GetByIdNoTrackingAsync(updatedChatMessage.Id).ConfigureAwait(false);

            var chatRoom = await roomService.GetByIdAsync(oldChatMessage.ChatRoomId).ConfigureAwait(false);

            if (oldChatMessage is null
                || chatRoom is null
                || (oldChatMessage.Text == updatedChatMessage.Text))
            {
                return NoContent();
            }

            var userRole = HttpContext.User.FindFirst("role")?.Value;

            if (userRole is null)
            {
                throw new ArgumentException($"{nameof(userRole)}");
            }

            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false)
                || (oldChatMessage.SenderRoleIsProvider != userRoleIsProvider))
            {
                return StatusCode(403, "You can change only text of your messages only in chat rooms where you participate.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (oldChatMessage.CreatedDateTime.CompareTo(DateTimeOffset.UtcNow.Subtract(whenMessageBecomesOld)) < 0)
            {
                return StatusCode(403, "Forbidden to change old messages.");
            }

            oldChatMessage.Text = updatedChatMessage.Text;

            var updatedMessage = await messageService.UpdateAsync(oldChatMessage).ConfigureAwait(false);

            return Ok(updatedMessage);
        }

        /// <summary>
        /// Delete ChatMessage by id.
        /// </summary>
        /// <param name="id">ChatMessage's id.</param>
        /// <returns>Status code.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMessageAsync(long id)
        {
            this.ValidateId(id, localizer);

            var messageThatIsDeleting = await messageService.GetByIdNoTrackingAsync(id).ConfigureAwait(false);
            var chatRoom = await roomService.GetByIdAsync(messageThatIsDeleting.ChatRoomId).ConfigureAwait(false);

            if (messageThatIsDeleting is null || chatRoom is null)
            {
                return NoContent();
            }

            var userRole = HttpContext.User.FindFirst("role")?.Value;

            if (userRole is null)
            {
                throw new ArgumentException($"{nameof(userRole)}");
            }

            bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!await this.UserHasRigtsForChatRoomAsync(chatRoom.WorkshopId, chatRoom.ParentId).ConfigureAwait(false)
                || (messageThatIsDeleting.SenderRoleIsProvider != userRoleIsProvider))
            {
                return StatusCode(403, "You can delete only your messages only in chat rooms where you participate.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (messageThatIsDeleting.CreatedDateTime.CompareTo(DateTimeOffset.UtcNow.Subtract(whenMessageBecomesOld)) < 0)
            {
                return StatusCode(403, "Forbidden to delete old messages.");
            }

            await messageService.DeleteAsync(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Delete ChatRoom by id.
        /// </summary>
        /// <param name="id">ChatRoom's id.</param>
        /// <returns>Status code.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRoomAsync(long id)
        {
            this.ValidateId(id, localizer);

            var room = await roomService.GetByIdAsync(id).ConfigureAwait(false);

            if (room is null)
            {
                return NoContent();
            }

            var roomMessages = await messageService.GetMessagesForChatRoomAsync(id, new OffsetFilter() { From = 0, Size = 1 }).ConfigureAwait(false);
            if (roomMessages.Any())
            {
                return StatusCode(403, "Forbidden to delete a chat room with chat messages.");
            }

            if (!await this.UserHasRigtsForChatRoomAsync(room.WorkshopId, room.ParentId).ConfigureAwait(false))
            {
                return StatusCode(403, "You can delete only chat rooms where you participate.");
            }

            await roomService.DeleteAsync(id).ConfigureAwait(false);

            return NoContent();
        }

        private async Task<bool> UserHasRigtsForChatRoomAsync(long workshopId, long parentId)
        {
            try
            {
                var userId = HttpContext.User.FindFirst("sub")?.Value;
                var userRole = HttpContext.User.FindFirst("role")?.Value;

                if (userId is null || userRole is null)
                {
                    throw new ArgumentException($"{nameof(userId)} or {nameof(userRole)}");
                }

                bool userRoleIsProvider = userRole.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

                if (userRoleIsProvider)
                {
                    // the user is Provider
                    // and we check if user is owner of workshop
                    var result = await validationService.UserIsWorkshopOwnerAsync(userId, workshopId).ConfigureAwait(false);
                    return result;
                }
                else
                {
                    // the user is Parent
                    // and we check if user is owner of parent
                    var result = await validationService.UserIsParentOwnerAsync(userId, parentId).ConfigureAwait(false);
                    return result;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
