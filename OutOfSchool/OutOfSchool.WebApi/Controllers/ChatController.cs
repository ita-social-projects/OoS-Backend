using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    public class ChatController : ControllerBase
    {
        private readonly IChatMessageService messageService;
        private readonly IChatRoomService roomService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class.
        /// </summary>
        /// <param name="messageService">Service for ChatMessage model.</param>
        /// <param name="roomService">Service for ChatRoom model.</param>
        /// <param name="localizer">Localizer.</param>
        public ChatController(IChatMessageService messageService, IChatRoomService roomService, IStringLocalizer<SharedResource> localizer)
        {
            this.messageService = messageService;
            this.roomService = roomService;
            this.localizer = localizer;
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
        public async Task<IActionResult> CreateMessage(ChatNewMessageDto chatNewMessageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var senderUserId = User.FindFirst("sub")?.Value;

            var chatMessageDto = new ChatMessageDto()
            {
                UserId = senderUserId,
                ChatRoomId = 0,
                Text = chatNewMessageDto.Text,
                CreatedTime = DateTimeOffset.Now,
                IsRead = false,
            };

            if (chatNewMessageDto.ChatRoomId > 0)
            {
                var existingChatRoom = await roomService.GetById(chatNewMessageDto.ChatRoomId).ConfigureAwait(false);
                if (!(existingChatRoom is null) && existingChatRoom.Users.Any(u => u.Id == senderUserId))
                {
                    chatMessageDto.ChatRoomId = existingChatRoom.Id;
                }
                else
                {
                    return StatusCode(403, "Forbidden to write messages to a chat room you are not participating in.");
                }
            }
            else
            {
                var chatIsPossible = await roomService.UsersCanChatBetweenEachOther(senderUserId, chatNewMessageDto.ReceiverUserId, chatNewMessageDto.WorkshopId).ConfigureAwait(false);
                if (!chatIsPossible)
                {
                    return StatusCode(403, "Chat is forbidden between these users.");
                }

                var chatRoomDto = await roomService.CreateOrReturnExisting(
                senderUserId, chatNewMessageDto.ReceiverUserId, chatNewMessageDto.WorkshopId)
                .ConfigureAwait(false);

                chatMessageDto.ChatRoomId = chatRoomDto.Id;
            }

            var createdMessageDto = await messageService.Create(chatMessageDto).ConfigureAwait(false);

            return CreatedAtAction(
                 nameof(GetMessageById),
                 new { id = createdMessageDto.Id },
                 createdMessageDto);
        }

        /// <summary>
        /// Get ChatMessage by Id.
        /// </summary>
        /// <param name="id">ChatMessage's Id.</param>
        /// <returns>ChatMessage that was found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatMessageDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMessageById(long id)
        {
            this.ValidateId(id, localizer);

            var message = await messageService.GetById(id).ConfigureAwait(false);

            if (message is null)
            {
                return NoContent();
            }
            else
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.Equals(userId, message.UserId, StringComparison.Ordinal))
                {
                    return Ok(message);
                }
                else
                {
                    return StatusCode(403, "Forbidden to get messages of another users.");
                }
            }
        }

        /// <summary>
        /// Get ChatRoom with ChatMessages by ChatRoomId.
        /// </summary>
        /// <param name="id">ChatRoom's Id.</param>
        /// <returns>ChatRoom that was found with its chat messages.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatRoomDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoomById(long id)
        {
            this.ValidateId(id, localizer);

            var userId = User.FindFirst("sub")?.Value;

            var chatRoom = await roomService.GetById(id).ConfigureAwait(false);
            if (chatRoom is null)
            {
                return NoContent();
            }
            else
            {
                if (chatRoom.Users.Any(u => u.Id == userId))
                {
                    return Ok(chatRoom);
                }
                else
                {
                    return StatusCode(403, "Forbidden to read a chat room of another users.");
                }
            }
        }

        /// <summary>
        /// Get a list of ChatRooms for current user.
        /// </summary>
        /// <returns>List of ChatRooms, messages are not loaded.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatRoomDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersRooms()
        {
            var userId = User.FindFirst("sub")?.Value;

            var chatRooms = await roomService.GetByUserId(userId).ConfigureAwait(false);

            var newChatRooms = new List<ChatRoomDto>();

            foreach (var room in chatRooms)
            {
                var notReadMessages = await messageService.GetAllNotReadByUserInChatRoom(room.Id, userId).ConfigureAwait(false);
                room.NotReadMessagesCount = notReadMessages.Count();
                newChatRooms.Add(room);
            }

            return Ok(newChatRooms);
        }

        /// <summary>
        /// Set status IsRead on true for each user's ChatMessage in the ChatRoom.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's Id.</param>
        /// <returns>List of successfully updated items.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ChatMessageDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessagesStatus(long chatRoomId)
        {
            this.ValidateId(chatRoomId, localizer);

            var userId = User.FindFirst("sub")?.Value;

            var room = await roomService.GetById(chatRoomId).ConfigureAwait(false);

            if (room is null)
            {
                return NoContent();
            }

            if (!room.Users.Any(x => x.Id == userId))
            {
                return BadRequest($"User is not a participant of chat:{chatRoomId}");
            }

            var chatMessages = await messageService.GetAllNotReadByUserInChatRoom(chatRoomId, userId).ConfigureAwait(false);

            if (chatMessages.Any())
            {
                chatMessages = await messageService.UpdateIsRead(chatMessages).ConfigureAwait(false);
                return Ok(chatMessages);
            }
            else
            {
                return Ok(chatMessages);
            }
        }

        /// <summary>
        /// Update ChatMessage info.
        /// </summary>
        /// <param name="chatMessageDto">Entity that will be updated.</param>
        /// <returns>Successfully updated item.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChatMessageDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMessage(ChatMessageDto chatMessageDto)
        {
            this.ValidateId(chatMessageDto.Id, localizer);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("sub")?.Value;

            var oldChatMessage = await messageService.GetById(chatMessageDto.Id).ConfigureAwait(false);

            if (oldChatMessage is null)
            {
                return NoContent();
            }

            if (!string.Equals(userId, oldChatMessage.UserId, StringComparison.Ordinal))
            {
                return StatusCode(403, "Forbidden to change messages of another users.");
            }

            if (oldChatMessage.ChatRoomId != chatMessageDto.ChatRoomId)
            {
                return StatusCode(403, "Forbidden to change chat room.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (oldChatMessage.CreatedTime.CompareTo(DateTimeOffset.Now.Subtract(whenMessageBecomesOld)) < 0)
            {
                return StatusCode(403, "Forbidden to change old messages.");
            }

            oldChatMessage.Text = chatMessageDto.Text;

            var updatedMessage = await messageService.Update(oldChatMessage).ConfigureAwait(false);

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
        public async Task<IActionResult> DeleteMessage(long id)
        {
            this.ValidateId(id, localizer);

            var userId = User.FindFirst("sub")?.Value;

            var oldChatMessage = await messageService.GetById(id).ConfigureAwait(false);

            if (oldChatMessage is null)
            {
                return NoContent();
            }

            if (!string.Equals(userId, oldChatMessage.UserId, StringComparison.Ordinal))
            {
                return StatusCode(403, "Forbidden to delete messages of another users.");
            }

            var whenMessageBecomesOld = new TimeSpan(0, 10, 0);

            if (oldChatMessage.CreatedTime.CompareTo(DateTimeOffset.Now.Subtract(whenMessageBecomesOld)) < 0)
            {
                return StatusCode(403, "Forbidden to delete old messages.");
            }

            await messageService.Delete(id).ConfigureAwait(false);

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
        public async Task<IActionResult> DeleteRoom(long id)
        {
            this.ValidateId(id, localizer);

            var userId = User.FindFirst("sub")?.Value;

            var room = await roomService.GetById(id).ConfigureAwait(false);

            if (room is null)
            {
                return NoContent();
            }

            if (room.ChatMessages.Any())
            {
                return StatusCode(403, "Forbidden to delete a chat room with chat messages.");
            }

            if (!room.Users.Any(x => x.Id == userId))
            {
                return StatusCode(403, "Forbidden to delete a chat room of another users.");
            }

            await roomService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}
