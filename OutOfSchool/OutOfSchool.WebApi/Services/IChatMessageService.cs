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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDto"/> that was created.</returns>
        Task<ChatMessageDto> Create(ChatMessageDto chatMessageDto);

        /// <summary>
        /// Get ChatMessage by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDto"/> that was found, or null.</returns>
        Task<ChatMessageDto> GetById(long id);

        /// <summary>
        /// Get ChatMessage with some ChatRoomId.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatMessageDto>> GetAllByChatRoomId(long chatRoomId);

        /// <summary>
        /// Update the ChatMessage.
        /// </summary>
        /// <param name="chatMessageDto">The ChatMessage to update.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageDto"/> that was updated.</returns>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<ChatMessageDto> Update(ChatMessageDto chatMessageDto);

        /// <summary>
        /// Update ChatMessages' property "IsRead".
        /// </summary>
        /// <param name="chatMessages">A List of ChatMessages that need to be updated.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements that were updated.</returns>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<IEnumerable<ChatMessageDto>> UpdateIsRead(IEnumerable<ChatMessageDto> chatMessages);

        /// <summary>
        /// Get ChatMessages that are not read.
        /// </summary>
        /// <param name="chatRoomId">Identifier of ChatRoom where ChatMessages are not read.</param>
        /// <param name="userId">Identifier of User who did not read ChatMessages.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatMessageDto>> GetAllNotReadByUserInChatRoom(long chatRoomId, string userId);

        /// <summary>
        /// Delete the ChatMessage.
        /// </summary>
        /// <param name="id">ChatMessage's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity with id was not found in system.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task Delete(long id);
    }
}
