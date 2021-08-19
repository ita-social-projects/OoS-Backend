﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD operations for ChatRoom and ChatRoomUser.
    /// </summary>
    public interface IChatRoomService
    {
        /// <summary>
        /// Create new ChatRoom or returns existing ChatRoom in the system.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was created or found.</returns>
        /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
        /// <exception cref="DbUpdateException">If trying to create entity something was wrong. For example invalid foreign keys.</exception>
        Task<ChatRoomDto> CreateOrReturnExistingAsync(long workshopId, long parentId);

        /// <summary>
        /// Get ChatRoom by it's key, including Users and Messages.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was found, or null.</returns>
        Task<ChatRoomDto> GetByIdAsync(long id);

        /// <summary>
        /// Get ChatRooms with last message and count of not read messages by specified Parent.
        /// </summary>
        /// <param name="parentId">Parent's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomWithLastMessage>> GetByParentIdAsync(long parentId);

        /// <summary>
        /// Get ChatRooms with last message and count of not read messages by specified Provider.
        /// </summary>
        /// <param name="providerId">Provider's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomWithLastMessage>> GetByProviderIdAsync(long providerId);

        /// <summary>
        /// Delete the ChatRoom including its messages.
        /// </summary>
        /// <param name="id">ChatRoom's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity with specified id was not found in system.</exception>
        /// <exception cref="InvalidOperationException">If there was more then one entity found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task DeleteAsync(long id);

        /// <summary>
        /// Get the ChatRoom by userIds and workshop. Not include ChatMessages.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was found, or null.</returns>
        /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
        Task<ChatRoomDto> GetUniqueChatRoomAsync(long workshopId, long parentId);
    }
}
