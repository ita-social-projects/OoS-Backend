using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private IEntityRepository<ChatRoom> roomRepository;
        private IEntityRepository<ChatRoomUser> roomUserRepository;
        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomService"/> class.
        /// </summary>
        /// <param name="chatRoomRepository">Repository for the ChatRoom entity.</param>
        /// <param name="chatRoomUserRepository">Repository for the ChatRoomUser entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatRoomService(IEntityRepository<ChatRoom> chatRoomRepository, IEntityRepository<ChatRoomUser> chatRoomUserRepository, ILogger logger)
        {
            this.roomRepository = chatRoomRepository;
            this.roomUserRepository = chatRoomUserRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDTO> CreateOrReturnExisting(string user1Id, string user2Id, long workshopId)
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
                    return await Create(user1Id, user2Id, workshopId).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"ChatRoom deleting was started. ChatRoom id:{id}");

            try
            {
                var query = roomRepository.Get<long>(includeProperties: "ChatMessages, ChatRoomUsers", where: x => x.Id == id);
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
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"Deleting ChatRoom id:{id} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDTO> GetByIdIncludeChatMessages(long id)
        {
            logger.Information($"Process of getting ChatRoom with its ChatMessages by Id:{id} was started.");

            try
            {
                var query = roomRepository.Get<long>(includeProperties: "ChatMessages, Users", where: x => x.Id == id);
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
            catch (Exception ex)
            {
                logger.Error($"Getting ChatRoom with id:{id} including ChatMessages failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomDTO>> GetByUserId(string userId)
        {
            logger.Information($"Process of getting all ChatRooms with userId:{userId} was started.");

            try
            {
                var chatRooms = await roomUserRepository
                    .Get<string>(includeProperties: "ChatRoom", where: x => x.UserId == userId)
                    .Select(x => x.ChatRoom)
                    .ToListAsync()
                    .ConfigureAwait(false);

                logger.Information(!chatRooms.Any()
                ? $"There is no ChatRoom in the system with userId:{userId}."
                : $"Successfully got all {chatRooms.Count} records with userId:{userId}.");

                return chatRooms.Select(item => item.ToModel()).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Getting all ChatMessages with userId:{userId} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDTO> GetUniqueChatRoomBetweenUsersWithinWorkshop(string user1Id, string user2Id, long workshopId)
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
                    var exmessage = "Logic error! More then 1(one) record was found when expected to exist only one.";
                    throw new Exception(exmessage);
                }

                var chatRoom = chatRooms.Count == 1 ? chatRooms.First() : null;

                return chatRoom.ToModel();
            }
            catch (Exception ex)
            {
                logger.Error($"Getting all ChatMessages with {nameof(user1Id)}:{user1Id}, {nameof(user2Id)}:{user2Id}, workshopId:{workshopId} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new ChatRoom without checking if it exists.
        /// </summary>
        /// <param name="user1Id">Id of User who is a participant of the workshop.</param>
        /// <param name="user2Id">Id of User who is owner(manager) of the workshop.</param>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDTO"/> that was created.</returns>
        private async Task<ChatRoomDTO> Create(string user1Id, string user2Id, long workshopId)
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
            catch (DbUpdateException ex)
            {
                logger.Error($"ChatRoom was not created. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
