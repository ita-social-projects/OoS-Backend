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
    public class ChatController : ControllerBase
    {
        private readonly IChatMessageService messageService;
        private readonly IChatRoomService roomService;
        private readonly IProviderService providerService;
        private readonly IParentService parentService;
        private readonly IWorkshopService workshopService;
        private readonly IStringLocalizer<SharedResource> localizer;

        private ParentDTO parent;
        private ProviderDto provider;
        private WorkshopDTO workshop;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class.
        /// </summary>
        /// <param name="messageService">Service for ChatMessage entities.</param>
        /// <param name="roomService">Service for ChatRoom entities.</param>
        /// <param name="providerService">Service for Provider entities.</param>
        /// <param name="parentService">Service for Parent entities.</param>
        /// <param name="workshopService">Service for Workshop entities.</param>
        /// <param name="localizer">Localizer.</param>
        public ChatController(IChatMessageService messageService, IChatRoomService roomService, IProviderService providerService, IParentService parentService, IWorkshopService workshopService, IStringLocalizer<SharedResource> localizer)
        {
            this.messageService = messageService;
            this.roomService = roomService;
            this.localizer = localizer;
            this.providerService = providerService;
            this.parentService = parentService;
            this.workshopService = workshopService;
        }

        /// <summary>
        /// Create new ChatMessage.
        /// </summary>
        /// <param name="chatNewMessageDto">Entity that contains text of message, receiver and workshop info.</param>
        /// <returns>Created <see cref="ChatMessageDto"/>.</returns>
        [Obsolete("This method is for testing purposes.")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ChatMessageDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMessageAsync(ChatMessageCreateDto chatNewMessageDto)
        {
            if (chatNewMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatNewMessageDto)}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // create new dto object that will be saved to the database
            var chatMessageDtoThatWillBeSaved = new ChatMessageDto()
            {
                SenderRoleIsProvider = chatNewMessageDto.SenderRoleIsProvider,
                Text = chatNewMessageDto.Text,
                CreatedTime = DateTimeOffset.UtcNow,
                IsRead = false,
                ChatRoomId = 0,
            };

            var existingRoom = await roomService.GetUniqueChatRoomAsync(chatNewMessageDto.WorkshopId, chatNewMessageDto.ParentId)
                .ConfigureAwait(false);

            // set the unique ChatRoomId property according to WorkshopId and ParentId
            if (existingRoom is null)
            {
                // validate received parameters
                var messageIsValid = await this.ValidateNewMessageAsync(chatNewMessageDto).ConfigureAwait(false);
                if (!messageIsValid)
                {
                    string message = "Some of the message parameters were wrong. There are no entities with such Ids or Ids do not belong to sender.";
                    return StatusCode(403, message);
                }

                var newChatRoomDto = await roomService.CreateOrReturnExistingAsync(chatNewMessageDto.WorkshopId, chatNewMessageDto.ParentId)
                    .ConfigureAwait(false);
                chatMessageDtoThatWillBeSaved.ChatRoomId = newChatRoomDto.Id;
            }
            else
            {
                // validate received parameters
                bool userHarRights = await this.UserHasRigtsForChatRoomAsync(existingRoom).ConfigureAwait(false);
                bool userRoleIsProvider = string.Equals(HttpContext.User.FindFirst("role")?.Value, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);
                if (!userHarRights || chatNewMessageDto.SenderRoleIsProvider != userRoleIsProvider)
                {
                    string message = "Some of the message parameters were wrong. User has no rights for this chat room or sender role is set invalid.";
                    return StatusCode(403, message);
                }

                chatMessageDtoThatWillBeSaved.ChatRoomId = existingRoom.Id;
            }

            // Save chatMessage in the system.
            var createdMessageDto = await messageService.CreateAsync(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);

            return new CreatedResult(string.Empty, createdMessageDto);
        }

        /// <summary>
        /// Get ChatRoom without ChatMessages by ChatRoomId.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>ChatRoom that was found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomDto))]
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
            else
            {
                if (await this.UserHasRigtsForChatRoomAsync(chatRoom).ConfigureAwait(false))
                {
                    return Ok(chatRoom);
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatRoomWithLastMessage>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersRoomsAsync()
        {
            var userRole = User.FindFirst("role")?.Value;
            var userId = User.FindFirst("sub")?.Value;

            IEnumerable<ChatRoomWithLastMessage> chatRooms;

            if (string.Equals(userRole, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);
                chatRooms = await roomService.GetByProviderIdAsync(provider.Id).ConfigureAwait(false);
            }
            else
            {
                var parent = await parentService.GetByUserId(userId).ConfigureAwait(false);
                chatRooms = await roomService.GetByParentIdAsync(parent.Id).ConfigureAwait(false);
            }

            if (!chatRooms.Any())
            {
                return NoContent();
            }

            return Ok(chatRooms);
        }

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

            if (!await this.UserHasRigtsForChatRoomAsync(room).ConfigureAwait(false))
            {
                return BadRequest($"User is not a participant of the chat:{chatRoomId}");
            }

            bool userRoleIsProvider = string.Equals(HttpContext.User.FindFirst("role")?.Value, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatMessageDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessageAsync(ChatMessageUpdateDto updatedChatMessage)
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

            bool userRoleIsProvider = string.Equals(HttpContext.User.FindFirst("role")?.Value, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!await this.UserHasRigtsForChatRoomAsync(chatRoom).ConfigureAwait(false)
                || (oldChatMessage.SenderRoleIsProvider != userRoleIsProvider))
            {
                return StatusCode(403, "You can change only text of your messages only in chat rooms where you participate.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (oldChatMessage.CreatedTime.CompareTo(DateTimeOffset.UtcNow.Subtract(whenMessageBecomesOld)) < 0)
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

            bool userRoleIsProvider = string.Equals(HttpContext.User.FindFirst("role")?.Value, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase);

            if (!await this.UserHasRigtsForChatRoomAsync(chatRoom).ConfigureAwait(false)
                || (messageThatIsDeleting.SenderRoleIsProvider != userRoleIsProvider))
            {
                return StatusCode(403, "You can delete only your messages only in chat rooms where you participate.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (messageThatIsDeleting.CreatedTime.CompareTo(DateTimeOffset.UtcNow.Subtract(whenMessageBecomesOld)) < 0)
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

            if (!await this.UserHasRigtsForChatRoomAsync(room).ConfigureAwait(false))
            {
                return StatusCode(403, "You can delete only chat rooms where you participate.");
            }

            await roomService.DeleteAsync(id).ConfigureAwait(false);

            return NoContent();
        }

        private async Task SetProviderParentAndWorkshopFieldsAccordingToSenderRoleAsync(long workshopId, long parentId)
        {
            var userRole = HttpContext.User.FindFirst("role")?.Value;
            if (Role.Provider.ToString().Equals(userRole, StringComparison.OrdinalIgnoreCase))
            {
                // if Sender role is Provider
                // so all fields need to be settled for future validation if provider is workshop's owner
                provider = await providerService.GetByUserId(HttpContext.User.FindFirst("sub")?.Value).ConfigureAwait(false);
                workshop = await workshopService.GetById(workshopId).ConfigureAwait(false);
                parent = await parentService.GetById(parentId).ConfigureAwait(false);

                if (provider is null || parent is null || workshop is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(provider)}, {nameof(parent)}, {nameof(workshop)}");
                }
            }
            else
            {
                // if Sender role is Parent
                // so only workshop and parent need to be checked if they exist in the system and Message.ParentId is right
                parent = await parentService.GetByUserId(HttpContext.User.FindFirst("sub")?.Value).ConfigureAwait(false);
                workshop = await workshopService.GetById(workshopId).ConfigureAwait(false);

                if (parent is null || workshop is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(parent)}, {nameof(workshop)}");
                }
            }
        }

        private async Task<bool> ValidateNewMessageAsync(ChatMessageCreateDto newMessage)
        {
            if (newMessage is null)
            {
                throw new ArgumentNullException($"{nameof(newMessage)}");
            }

            try
            {
                await this.SetProviderParentAndWorkshopFieldsAccordingToSenderRoleAsync(newMessage.WorkshopId, newMessage.ParentId).ConfigureAwait(false);
            }
            catch
            {
                return false;
            }

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

        private async Task SetProviderAndWorkshopFieldsOrParentFieldAccordingToUserRoleAsync(long workshopId)
        {
            var userRole = HttpContext.User.FindFirst("role")?.Value;
            if (Role.Provider.ToString().Equals(userRole, StringComparison.OrdinalIgnoreCase))
            {
                // if User role is Provider
                // so provider and workshop fields need to be settled for future validation if provider is workshop's owner
                provider = await providerService.GetByUserId(HttpContext.User.FindFirst("sub")?.Value).ConfigureAwait(false);
                workshop = await workshopService.GetById(workshopId).ConfigureAwait(false);

                if (provider is null || workshop is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(provider)}, {nameof(workshop)}");
                }
            }
            else
            {
                // if User role is Parent
                // so only parent need to be checked if he has rights to get information
                parent = await parentService.GetByUserId(HttpContext.User.FindFirst("sub")?.Value).ConfigureAwait(false);

                if (parent is null)
                {
                    throw new ArgumentException($"Some of entites were not found: {nameof(parent)}");
                }
            }
        }

        private async Task<bool> UserHasRigtsForChatRoomAsync(ChatRoomDto chatRoomDto)
        {
            if (chatRoomDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatRoomDto)}");
            }

            await this.SetProviderAndWorkshopFieldsOrParentFieldAccordingToUserRoleAsync(chatRoomDto.WorkshopId).ConfigureAwait(false);

            if (provider is null)
            {
                // if provider is not set to instance, so the user is Parent
                // and we check if ParentId is equal to Sender Parent.Id
                // and SenderRole must be Parent
                if (chatRoomDto.ParentId != parent.Id)
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
                // if provider is set to instance, so the user is Provider
                // and we check if ProviderId equal user Provider.Id
                if (workshop.ProviderId != provider.Id)
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
