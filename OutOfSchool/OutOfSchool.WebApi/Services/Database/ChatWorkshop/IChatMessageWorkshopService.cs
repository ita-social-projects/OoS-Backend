using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
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
        /// <param name="chatMessageCreateDto">ChatMessage to create.</param>
        /// <param name="userRole">The role of sender (parent/provider).</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="ChatMessageWorkshopDto"/> that was created.</returns>
        /// <exception cref="ArgumentNullException">If the parameter <see cref="ChatMessageWorkshopDto"/> was not set to instance.</exception>
        Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopCreateDto chatMessageCreateDto, Role userRole);

        /// <summary>
        /// Get a portion of all ChatMessages with specified ChatRoomId.
        /// </summary>
        /// <param name="chatRoomId">ChatRoom's key.</param>
        /// <param name="offsetFilter">Filter to take specified part of entities.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatMessageDTO}"/> that contains elements from the input sequence.</returns>
        Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(long chatRoomId, OffsetFilter offsetFilter);

        /// <summary>
        /// Update ChatMessages' property "IsRead" in specified ChatRoom and specified User.
        /// </summary>
        /// <param name="chatRoomId">The key of ChatRoom.</param>
        /// <param name="userRole">The role of sender (parent/provider).</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a number of messages that were updated.</returns>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<int> UpdateIsReadByCurrentUserInChatRoomAsync(long chatRoomId, Role userRole);
    }
}
