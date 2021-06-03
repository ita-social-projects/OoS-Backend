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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was created.</returns>
        /// <exception cref="ArgumentException">If chat is forbidden between users.</exception>
        /// <exception cref="InvalidOperationException">If one of the entities was not found.</exception>
        Task<ChatRoomDto> CreateOrReturnExisting(string user1Id, string user2Id, long workshopId);

        /// <summary>
        /// Get ChatRoom by it's key, including Users and Messages.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was found, or null.</returns>
        Task<ChatRoomDto> GetById(long id);

        /// <summary>
        /// Get ChatRooms, Not including ChatMessages for some User.
        /// </summary>
        /// <param name="userId">User's identifier.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{ChatRoomDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<ChatRoomDto>> GetByUserId(string userId);

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
        /// <param name="workshopId">Id of the Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was found, or null.</returns>
        /// <exception cref="InvalidOperationException">If the logic of creating chat was compromised.</exception>
        Task<ChatRoomDto> GetUniqueChatRoomBetweenUsersWithinWorkshop(string user1Id, string user2Id, long workshopId);

        /// <summary>
        /// Validate if users can chat between each other.
        /// </summary>
        /// <param name="user1Id">Id of one User.</param>
        /// <param name="user2Id">Id of another User.</param>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains <see cref="bool"/> representing if chat can be created.</returns>
        /// <exception cref="ArgumentException">If chat is forbidden between users.</exception>
        /// <exception cref="InvalidOperationException">If one of the entities was not found.</exception>
        Task<bool> ValidateUsers(string user1Id, string user2Id, long workshopId);
    }
}
