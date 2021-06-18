using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service works with two repositories for CRUD operations for <see cref="ChatRoom"/> and <see cref="ChatRoomUser"/>.
    /// </summary>
    public class ChatRoomService : IChatRoomService
    {
        private readonly IEntityRepository<ChatRoom> roomRepository;
        private readonly IEntityRepository<User> userRepository;
        private readonly IWorkshopRepository workshopRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomService"/> class.
        /// </summary>
        /// <param name="chatRoomRepository">Repository for the ChatRoom entity.</param>
        /// <param name="userRepository">Repository for the User entity.</param>
        /// <param name="workshopRepository">Repository for the Workshop entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatRoomService(
            IEntityRepository<ChatRoom> chatRoomRepository,
            IEntityRepository<User> userRepository,
            IWorkshopRepository workshopRepository,
            ILogger logger)
        {
            this.roomRepository = chatRoomRepository;
            this.userRepository = userRepository;
            this.workshopRepository = workshopRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> CreateOrReturnExisting(string user1Id, string user2Id, long workshopId)
        {
            logger.Information($"Checking a ChatRoom with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started.");

            try
            {
                var existingChatRoom = await this.GetUniqueChatRoomBetweenUsersWithinWorkshop(user1Id, user2Id, workshopId).ConfigureAwait(false);

                if (!(existingChatRoom is null))
                {
                    logger.Information($"ChatRoom id:{existingChatRoom.Id} is already existing in the system.");
                    return existingChatRoom;
                }
                else
                {
                    return await this.Create(user1Id, user2Id, workshopId).ConfigureAwait(false);
                }
            }
            catch (ArgumentException exception)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild validation: {exception.Message}");
                throw;
            }
            catch (InvalidOperationException exception)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
            catch (Exception exception)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"ChatRoom deleting was started. ChatRoom id:{id}");

            try
            {
                var query = roomRepository.Get<long>(includeProperties: "ChatMessages,ChatRoomUsers", where: x => x.Id == id);
                var chatRooms = await query.ToListAsync().ConfigureAwait(false);
                var chatRoom = chatRooms.Count == 1 ? chatRooms.First() : null;

                if (!(chatRoom is null))
                {
                    await roomRepository.Delete(chatRoom).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(id), $"ChatRoom with id:{id} was not found in the system.");
                }

                logger.Information($"ChatRoom id:{id} was successfully deleted.");
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Deleting ChatRoom id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> GetById(long id)
        {
            logger.Information($"Process of getting ChatRoom by Id:{id} was started.");

            try
            {
                var query = roomRepository.GetByFilterNoTracking(x => x.Id == id, includeProperties: "Users,ChatMessages");
                var chatRooms = await query.ToListAsync().ConfigureAwait(false);
                var chatRoom = chatRooms.Count == 1 ? chatRooms.First() : null;

                if (chatRoom is null)
                {
                    logger.Information($"ChatRoom with id:{id} was not found.");
                    return null;
                }
                else
                {
                    logger.Information($"ChatRoom id:{chatRoom.Id} was successfully found.");
                    return chatRoom.ToModel();
                }
            }
            catch (Exception exception)
            {
                logger.Error($"Getting ChatRoom with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomDto>> GetByUserId(string userId)
        {
            logger.Information($"Process of getting all ChatRooms with userId:{userId} was started.");

            try
            {
                var query = roomRepository.GetByFilterNoTracking(x => x.Users.Any(u => u.Id == userId), includeProperties: "Users");
                var chatRooms = await query.AsNoTracking().ToListAsync().ConfigureAwait(false);

                logger.Information(!chatRooms.Any()
                ? $"There is no ChatRoom in the system with userId:{userId}."
                : $"Successfully got all {chatRooms.Count} records with userId:{userId}.");

                return chatRooms.Select(x => x.ToModelWithoutChatMessages());
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all ChatMessages with userId:{userId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> GetUniqueChatRoomBetweenUsersWithinWorkshop(string user1Id, string user2Id, long workshopId)
        {
            logger.Information($"Process of getting unique ChatRoom with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started.");

            try
            {
                var chatRooms = await roomRepository
                    .Get<string>(includeProperties: "Users", where: x => x.WorkshopId == workshopId)
                    .Where(x => x.Users.All(u => u.Id == user1Id || u.Id == user2Id))
                    .ToListAsync()
                    .ConfigureAwait(false);

                logger.Information(!chatRooms.Any()
                ? $"There is no ChatRoom in the system with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started.."
                : $"Successfully got {chatRooms.Count} record(s) with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started..");

                if (chatRooms.Count > 1)
                {
                    var exmessage = $"Logic error! {chatRooms.Count} record was found when expected to exist only one.";
                    throw new InvalidOperationException(exmessage);
                }

                var chatRoom = chatRooms.Count == 1 ? chatRooms.First() : null;

                return chatRoom.ToModel();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all ChatMessages with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UsersCanChatBetweenEachOther(string user1Id, string user2Id, long workshopId)
        {
            logger.Information($"Validation of ChatRoom creating with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started.");
            
            try
            {
                var users1 = await userRepository.GetByFilter(u => u.Id == user1Id).ConfigureAwait(false);
                var users2 = await userRepository.GetByFilter(u => u.Id == user2Id).ConfigureAwait(false);
                var workshops = await workshopRepository.GetByFilter(w => w.Id == workshopId).ConfigureAwait(false);

                var user1 = users1.First();
                var user2 = users2.First();
                var workshop = workshops.First();

                // Forbid chats between users with the same role. But parent still can write admin.
                if (string.Equals(user1.Role, user2.Role, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Forbid chats when workshop is not managed by one of the users.
                if (string.Equals(user1.Role, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    (!string.Equals(workshop.Provider.UserId, user1.Id, StringComparison.Ordinal)))
                {
                    return false;
                }
                else if (string.Equals(user2.Role, Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    (!string.Equals(workshop.Provider.UserId, user2.Id, StringComparison.Ordinal)))
                {
                    return false;
                }

                // Forbid chats between parent and admin.
                if ((string.Equals(user1.Role, Role.Parent.ToString(), StringComparison.OrdinalIgnoreCase) && string.Equals(user2.Role, Role.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
                    || (string.Equals(user2.Role, Role.Parent.ToString(), StringComparison.OrdinalIgnoreCase) && string.Equals(user1.Role, Role.Admin.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                return true;
            }
            catch (InvalidOperationException exception)
            {
                logger.Error($"One of the entities was not found. {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new ChatRoom without checking if it exists.
        /// </summary>
        /// <param name="user1Id">Id of one User.</param>
        /// <param name="user2Id">Id of another User.</param>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was created.</returns>
        private async Task<ChatRoomDto> Create(string user1Id, string user2Id, long workshopId)
        {
            logger.Information($"ChatRoom creating with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} was started.");

            var chatRoom = new ChatRoom()
            {
                WorkshopId = workshopId,
                ChatRoomUsers = new List<ChatRoomUser>(),
            };
            chatRoom.ChatRoomUsers.Add(new ChatRoomUser() { UserId = user1Id });
            chatRoom.ChatRoomUsers.Add(new ChatRoomUser() { UserId = user2Id });

            Func<Task<ChatRoom>> operation = async () => await roomRepository.Create(chatRoom).ConfigureAwait(false);

            try
            {
                var newChatRoom = await roomRepository.RunInTransaction(operation).ConfigureAwait(false);
                logger.Information($"ChatRoom id:{newChatRoom.Id} was saved to DB.");
                foreach (var item in newChatRoom.ChatRoomUsers)
                {
                    logger.Information($"ChatRoomUser id:{item.Id} was saved to DB.");
                }

                return chatRoom.ToModel();
            }
            catch (DbUpdateException exception)
            {
                logger.Error($"ChatRoom was not created. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
