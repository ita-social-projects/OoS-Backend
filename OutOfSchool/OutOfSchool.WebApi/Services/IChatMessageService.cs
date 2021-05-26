using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for ChatMessage entity.
    /// </summary>
    public interface IChatMessageService
    {
        /// <summary>
        /// Create new ChatMessage.
        /// </summary>
        /// <param name="chatMessageDto">ChatMessage to create.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDTO"/> that was created.</returns>
        Task<ChatMessageDTO> Create(ChatMessageDTO chatMessageDto);

        /// <summary>
        /// Get ChatMessage by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDTO"/> that was found, or null.</returns>
        Task<ChatMessageDTO> GetById(long id);

        /// <summary>
        /// Get ChatMessage with some ChatRoomId.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatMessageDTO>> GetAllByChatRoomId(long chatRoomId);

        /// <summary>
        /// Update the ChatMessage.
        /// </summary>
        /// <param name="chatMessageDto">The ChatMessage to update.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDTO"/> that was updated.</returns>
        Task<ChatMessageDTO> Update(ChatMessageDTO chatMessageDto);

        /// <summary>
        /// Update ChatMessages' property "IsRead".
        /// </summary>
        /// <param name="chatMessages">A List of ChatMessages that need to be updated.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements that were updated.</returns>
        Task<IEnumerable<ChatMessageDTO>> UpdateIsRead(IEnumerable<ChatMessageDTO> chatMessages);

        /// <summary>
        /// Get ChatMessages that are not read.
        /// </summary>
        /// <param name="chatRoomId">Identifier of ChatRoom where ChatMessages are not read.</param>
        /// <param name="userId">Identifier of User who did not read ChatMessages.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatMessageDTO>> GetAllNotReadByUserInChatRoom(long chatRoomId, string userId);

        /// <summary>
        /// Delete the ChatMessage.
        /// </summary>
        /// <param name="id">ChatMessage's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity with id was not found in system.</exception>
        Task Delete(long id);
    }
}
