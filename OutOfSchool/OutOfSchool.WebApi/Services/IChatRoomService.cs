using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// <param name="user1Id">Id of one User.</param>
        /// <param name="user2Id">Id of another User.</param>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDTO"/> that was created.</returns>
        Task<ChatRoomDTO> CreateOrReturnExisting(string user1Id, string user2Id, long workshopId);

        /// <summary>
        /// Get ChatRoom by it's key, including ChatMessages.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDTO"/> that was found, or null.</returns>
        Task<ChatRoomDTO> GetByIdIncludeChatMessages(long id);

        /// <summary>
        /// Get ChatRooms, Not including ChatMessages for some User.
        /// </summary>
        /// <param name="userId">User's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomDTO>> GetByUserId(string userId);

        /// <summary>
        /// Delete the ChatRoom including its messages.
        /// </summary>
        /// <param name="id">ChatRoom's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity with id was not found in system.</exception>
        Task Delete(long id);

        /// <summary>
        /// Get the ChatRoom by userIds and workshop. Not include ChatMessages.
        /// </summary>
        /// <param name="user1Id">Id of one User.</param>
        /// <param name="user2Id">Id of another User.</param>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDTO"/> that was found, or null.</returns>
        Task<ChatRoomDTO> GetUniqueChatRoomBetweenUsersWithinWorkshop(string user1Id, string user2Id, long workshopId);
    }
}
