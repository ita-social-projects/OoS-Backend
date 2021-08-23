using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for ChatMessage entity.
    /// </summary>
    public interface IChatMessageWorkshopService
    {
        /// <summary>
        /// Create new ChatMessage.
        /// </summary>
        /// <param name="chatMessageDto">ChatMessage to create.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ChatMessageWorkshopDto"/> that was created.</returns>
        /// <exception cref="ArgumentNullException">If the parameter <see cref="ChatMessageWorkshopDto"/> was not set to instance.</exception>
        Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopDto chatMessageDto);

        /// <summary>
        /// Delete the ChatMessage.
        /// </summary>
        /// <param name="id">ChatMessage's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity with id was not found in system.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task DeleteAsync(long id);

        /// <summary>
        /// Get ChatMessage by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageWorkshopDto"/> that was found, or null.</returns>
        Task<ChatMessageWorkshopDto> GetByIdNoTrackingAsync(long id);

        /// <summary>
        /// Update the ChatMessage.
        /// </summary>
        /// <param name="chatMessageDto">The ChatMessage to update.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatMessageWorkshopDto"/> that was updated.</returns>
        /// <exception cref="ArgumentNullException">If the parameter <see cref="ChatMessageWorkshopDto"/> was not set to instance.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<ChatMessageWorkshopDto> UpdateAsync(ChatMessageWorkshopDto chatMessageDto);

        /// <summary>
        /// Get a portion of all ChatMessages with specified ChatRoomId.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's key.</param>
        /// <param name="offsetFilter">Filter to take specified part of entities.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(long chatRoomId, OffsetFilter offsetFilter);

        /// <summary>
        /// Update ChatMessages' property "IsRead" in specified ChatRoom and specified User.
        /// </summary>
        /// <param name="chatRoomId">The key of ChatRoom.</param>
        /// <param name="currentUserRoleIsProvider">The role of current user.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a number of messages that were updated.</returns>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<int> UpdateIsReadByCurrentUserInChatRoomAsync(long chatRoomId, bool currentUserRoleIsProvider);
    }
}
