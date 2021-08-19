using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Service works with repositories for CRUD operations for <see cref = "ChatRoom" />.
    /// </summary>
    public class ChatRoomService : IChatRoomService
    {
        private readonly IEntityRepository<ChatRoom> roomRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomService"/> class.
        /// </summary>
        /// <param name="chatRoomRepository">Repository for the ChatRoom entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatRoomService(
            IEntityRepository<ChatRoom> chatRoomRepository,
            ILogger logger)
        {
            this.roomRepository = chatRoomRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> CreateOrReturnExistingAsync(long workshopId, long parentId)
        {
            logger.Information($"Checking a {nameof(ChatRoom)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            try
            {
                var existingChatRoom = await this.GetUniqueChatRoomAsync(workshopId, parentId).ConfigureAwait(false);

                if (!(existingChatRoom is null))
                {
                    logger.Information($"ChatRoom id:{existingChatRoom.Id} is already existing in the system.");
                    return existingChatRoom;
                }
                else
                {
                    var newChatRoom = await this.CreateAsync(workshopId, parentId).ConfigureAwait(false);
                    return newChatRoom;
                }
            }
            catch (InvalidOperationException exception)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
            catch (DbUpdateException exception)
            {
                logger.Error($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(long id)
        {
            logger.Information($"{nameof(ChatRoom)} {nameof(id)}:{id} deleting was started.");

            try
            {
                var query = roomRepository.Get<long>(includeProperties: $"{nameof(ChatRoom.ChatMessages)}", where: x => x.Id == id);
                var chatRooms = await query.ToListAsync().ConfigureAwait(false);
                var chatRoom = chatRooms.SingleOrDefault();

                if (chatRoom is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(id), $"{nameof(ChatRoom)} {nameof(id)}:{id} was not found in the system.");
                }
                else
                {
                    await roomRepository.Delete(chatRoom).ConfigureAwait(false);
                }

                logger.Information($"{nameof(ChatRoom)} {nameof(id)}:{id} was successfully deleted.");
            }
            catch (InvalidOperationException)
            {
                logger.Error($"The logic of creating a {nameof(ChatRoom)} was compromised. There is more than one {nameof(ChatRoom)} with {nameof(ChatRoom.Id)}:{id} in the system.");
                throw;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Deleting ChatRoom id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> GetByIdAsync(long id)
        {
            logger.Information($"Process of getting {nameof(ChatRoom)} by Id:{id} was started.");

            try
            {
                var chatRooms = await roomRepository.GetByFilter(
                    predicate: x => x.Id == id,
                    includeProperties: $"{nameof(ChatRoom.Parent)},{nameof(ChatRoom.Workshop)}")
                    .ConfigureAwait(false);

                var chatRoom = chatRooms.SingleOrDefault();

                return chatRoom?.ToModel();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting {nameof(ChatRoom)} with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomWithLastMessage>> GetByParentIdAsync(long parentId)
        {
            throw new NotImplementedException();

            // logger.Information($"Process of getting all {nameof(ChatRoom)}(s/es) with {nameof(parentId)}:{parentId} was started.");
            // try
            // {
            //    var query = roomRepository.GetByFilterNoTracking(x => x.Users.Any(u => u.Id == parentId), includeProperties: "Users");
            //    var chatRooms = await query.AsNoTracking().ToListAsync().ConfigureAwait(false);
            //    logger.Information(!chatRooms.Any()
            //    ? $"There is no ChatRoom in the system with userId:{parentId}."
            //    : $"Successfully got all {chatRooms.Count} records with userId:{parentId}.");
            //    return chatRooms.Select(x => x.ToModelWithChatMessages());
            // }
            // catch (Exception exception)
            // {
            //    logger.Error($"Getting all ChatMessages with userId:{parentId} failed. Exception: {exception.Message}");
            //    throw;
            // }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomWithLastMessage>> GetByProviderIdAsync(long providerId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDto> GetUniqueChatRoomAsync(long workshopId, long parentId)
        {
            logger.Information($"Process of getting unique {nameof(ChatRoom)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            try
            {
                var chatRooms = await roomRepository.GetByFilter(r => r.WorkshopId == workshopId && r.ParentId == parentId, $"{nameof(ChatRoom.Parent)},{nameof(ChatRoom.Workshop)}").ConfigureAwait(false);
                var chatRoom = chatRooms.SingleOrDefault();

                logger.Information(chatRoom is null
                    ? $"There is no {nameof(ChatRoom)} in the system with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId}."
                    : $"Successfully got a {nameof(ChatRoom)} with {nameof(chatRoom.Id)}:{chatRoom.Id}.");

                return chatRoom?.ToModel();
            }
            catch (InvalidOperationException)
            {
                logger.Error($"The logic of creating a {nameof(ChatRoom)} was compromised. There is more than one {nameof(ChatRoom)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} in the system.");
                throw;
            }
            catch (Exception exception)
            {
                logger.Error($"Getting {nameof(ChatRoom)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new ChatRoom without checking if it exists.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomDto"/> that was created.</returns>
        /// <exception cref="DbUpdateException">If an error is encountered while saving to database.</exception>
        private async Task<ChatRoomDto> CreateAsync(long workshopId, long parentId)
        {
            logger.Information($"{nameof(ChatRoom)} creating with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            var chatRoom = new ChatRoom()
            {
                WorkshopId = workshopId,
                ParentId = parentId,
            };

            try
            {
                var newChatRoom = await roomRepository.Create(chatRoom).ConfigureAwait(false);
                logger.Information($"{nameof(ChatRoom)} id:{newChatRoom.Id} was saved to DB.");

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
