using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD operations for ChatRoom and ChatRoomUser.
    /// </summary>
    public interface IChatRoomWorkshopService
    {
        /// <summary>
        /// Create new ChatRoom or returns existing ChatRoom in the system.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was created or found.</returns>
        /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
        /// <exception cref="DbUpdateException">If trying to create entity something was wrong. For example invalid foreign keys.</exception>
        Task<ChatRoomWorkshopDto> CreateOrReturnExistingAsync(long workshopId, long parentId);

        /// <summary>
        /// Get ChatRoom by it's key, including Users and Messages.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was found, or null.</returns>
        Task<ChatRoomWorkshopDto> GetByIdAsync(long id);

        /// <summary>
        /// Get ChatRooms with last message and count of not read messages by specified Parent.
        /// </summary>
        /// <param name="parentId">Parent's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByParentIdAsync(long parentId);

        /// <summary>
        /// Get ChatRooms with last message and count of not read messages by specified Provider.
        /// </summary>
        /// <param name="providerId">Provider's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByProviderIdAsync(long providerId);

        /// <summary>
        /// Get ChatRooms with last message and count of not read messages by specified Workshop.
        /// </summary>
        /// <param name="workshopId">Workshop's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomWithLastMessage}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdAsync(long workshopId);

        /// <summary>
        /// Get a list of ChatRoom's Ids by specified Parent.
        /// </summary>
        /// <param name="parentId">Parent's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{Int64}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<long>> GetChatRoomIdsByParentIdAsync(long parentId);

        /// <summary>
        /// Get a list of ChatRoom's Ids by specified Provider.
        /// </summary>
        /// <param name="providerId">Provider's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{Int64}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<long>> GetChatRoomIdsByProviderIdAsync(long providerId);

        /// <summary>
        /// Delete the ChatRoom including its messages.
        /// </summary>
        /// <param name="id">ChatRoom's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">If there was no one entity or more then one entity found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task DeleteAsync(long id);

        /// <summary>
        /// Get the ChatRoom by userIds and workshop. Not include ChatMessages.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was found, or null.</returns>
        /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
        Task<ChatRoomWorkshopDto> GetUniqueChatRoomAsync(long workshopId, long parentId);
    }
}
