using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models.ChatWorkshop;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service works with repositories for CRUD operations for <see cref = "ChatRoomWorkshop" />.
    /// </summary>
    public class ChatRoomWorkshopService : IChatRoomWorkshopService
    {
        private readonly IEntityRepository<ChatRoomWorkshop> roomRepository;
        private readonly IChatRoomWorkshopModelForChatListRepository roomWorkshopWithLastMessageRepository;
        private readonly ILogger<ChatRoomWorkshopService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomWorkshopService"/> class.
        /// </summary>
        /// <param name="chatRoomRepository">Repository for the ChatRoom entity.</param>
        /// <param name="roomWorkshopWithLastMessageRepository">Repository for the ChatRoom entity with special model.</param>
        /// <param name="logger">Logger.</param>
        public ChatRoomWorkshopService(
            IEntityRepository<ChatRoomWorkshop> chatRoomRepository,
            ILogger<ChatRoomWorkshopService> logger,
            IChatRoomWorkshopModelForChatListRepository roomWorkshopWithLastMessageRepository)
        {
            this.roomRepository = chatRoomRepository ?? throw new ArgumentNullException(nameof(chatRoomRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.roomWorkshopWithLastMessageRepository = roomWorkshopWithLastMessageRepository ?? throw new ArgumentNullException(nameof(roomWorkshopWithLastMessageRepository));
        }

        /// <inheritdoc/>
        public async Task<ChatRoomWorkshopDto> CreateOrReturnExistingAsync(long workshopId, long parentId)
        {
            logger.LogDebug($"Checking a {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            try
            {
                var existingChatRoom = await this.GetUniqueChatRoomAsync(workshopId, parentId).ConfigureAwait(false);

                if (existingChatRoom is null)
                {
                    var newChatRoom = await this.CreateAsync(workshopId, parentId).ConfigureAwait(false);
                    logger.LogDebug($"{nameof(ChatRoomWorkshop)} id:{newChatRoom.Id} was saved to DB.");
                    return newChatRoom.ToModel();
                }
                else
                {
                    logger.LogDebug($"ChatRoom id:{existingChatRoom.Id} is already existing in the system.");
                    return existingChatRoom;
                }
            }
            catch (InvalidOperationException exception)
            {
                logger.LogError($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
            catch (DbUpdateException exception)
            {
                logger.LogError($"CreateOrReturnExisting ChatRoom faild: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(long id)
        {
            logger.LogDebug($"{nameof(ChatRoomWorkshop)} {nameof(id)}:{id} deleting was started.");

            try
            {
                var query = roomRepository.Get<long>(includeProperties: $"{nameof(ChatRoomWorkshop.ChatMessages)}", where: x => x.Id == id);
                var chatRooms = await query.ToListAsync().ConfigureAwait(false);
                var chatRoom = chatRooms.Single();

                await roomRepository.Delete(chatRoom).ConfigureAwait(false);

                logger.LogDebug($"{nameof(ChatRoomWorkshop)} {nameof(id)}:{id} was successfully deleted.");
            }
            catch (InvalidOperationException exception)
            {
                logger.LogError($"Deleting a {nameof(ChatRoomWorkshop)} was failed. Exception: {exception.Message}");
                throw;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Deleting ChatRoom id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomWorkshopDto> GetByIdAsync(long id)
        {
            logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} by Id:{id} was started.");

            try
            {
                var chatRooms = await roomRepository.GetByFilter(
                    predicate: x => x.Id == id,
                    includeProperties: $"{nameof(ChatRoomWorkshop.Parent)},{nameof(ChatRoomWorkshop.Workshop)}")
                    .ConfigureAwait(false);

                var chatRoom = chatRooms.SingleOrDefault();

                return chatRoom?.ToModel();
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting {nameof(ChatRoomWorkshop)} with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByParentIdAsync(long parentId)
        {
            logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(parentId)}:{parentId} was started.");

            try
            {
                var rooms = await roomWorkshopWithLastMessageRepository.GetByParentIdAsync(parentId).ConfigureAwait(false);
                logger.LogDebug(rooms.Count > 0
                    ? $"There is no Chat rooms in the system with userId:{parentId}."
                    : $"Successfully got all {rooms.Count} records with userId:{parentId}.");
                return rooms.Select(x => x.ToModel());
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(parentId)}:{parentId}. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByProviderIdAsync(long providerId)
        {
            logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(providerId)}:{providerId} was started.");

            try
            {
                var rooms = await roomWorkshopWithLastMessageRepository.GetByProviderIdAsync(providerId).ConfigureAwait(false);
                logger.LogDebug(rooms.Count > 0
                    ? $"There is no Chat rooms in the system with userId:{providerId}."
                    : $"Successfully got all {rooms.Count} records with userId:{providerId}.");
                return rooms.Select(x => x.ToModel());
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(providerId)}:{providerId}. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdAsync(long workshopId)
        {
            logger.Debug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(workshopId)}:{workshopId} was started.");

            try
            {
                var rooms = await roomWorkshopWithLastMessageRepository.GetByWorkshopIdAsync(workshopId).ConfigureAwait(false);
                logger.LogDebug(rooms.Count > 0
                    ? $"There is no Chat rooms in the system with userId:{workshopId}."
                    : $"Successfully got all {rooms.Count} records with userId:{workshopId}.");
                return rooms.Select(x => x.ToModel());
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(workshopId)}:{workshopId}. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<long>> GetChatRoomIdsByParentIdAsync(long parentId)
        {
            logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} Ids with {nameof(parentId)}:{parentId} was started.");

            try
            {
                var rooms = await roomRepository.GetByFilter(r => r.ParentId == parentId).ConfigureAwait(false);
                logger.LogDebug(!rooms.Any()
                    ? $"There is no Chat rooms in the system with userId:{parentId}."
                    : $"Successfully got all {rooms.Count()} records with userId:{parentId}.");
                return rooms.Select(x => x.Id);
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatRoomWorkshop)} Ids with {nameof(parentId)}:{parentId}. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<long>> GetChatRoomIdsByProviderIdAsync(long providerId)
        {
            logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} Ids with {nameof(providerId)}:{providerId} was started.");

            try
            {
                var rooms = await roomRepository.GetByFilter(r => r.Workshop.ProviderId == providerId).ConfigureAwait(false);
                logger.LogDebug(!rooms.Any()
                    ? $"There is no Chat rooms in the system with userId:{providerId}."
                    : $"Successfully got all {rooms.Count()} records with userId:{providerId}.");
                return rooms.Select(x => x.Id);
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatRoomWorkshop)} Ids with {nameof(providerId)}:{providerId}. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomWorkshopDto> GetUniqueChatRoomAsync(long workshopId, long parentId)
        {
            logger.LogDebug($"Process of getting unique {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            try
            {
                var chatRooms = await roomRepository.GetByFilter(r => r.WorkshopId == workshopId && r.ParentId == parentId, $"{nameof(ChatRoomWorkshop.Parent)},{nameof(ChatRoomWorkshop.Workshop)}").ConfigureAwait(false);
                var chatRoom = chatRooms.SingleOrDefault();

                logger.LogDebug(chatRoom is null
                    ? $"There is no {nameof(ChatRoomWorkshop)} in the system with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId}."
                    : $"Successfully got a {nameof(ChatRoomWorkshop)} with {nameof(chatRoom.Id)}:{chatRoom.Id}.");

                return chatRoom?.ToModel();
            }
            catch (InvalidOperationException)
            {
                logger.LogError($"The logic of creating a {nameof(ChatRoomWorkshop)} was compromised. There is more than one {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} in the system.");
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create new ChatRoom without checking if it exists.
        /// </summary>
        /// <param name="workshopId">Id of Workshop.</param>
        /// <param name="parentId">Id of Parent.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="ChatRoomWorkshopDto"/> that was created.</returns>
        /// <exception cref="DbUpdateException">If an error is encountered while saving to database.</exception>
        private Task<ChatRoomWorkshop> CreateAsync(long workshopId, long parentId)
        {
            logger.LogDebug($"{nameof(ChatRoomWorkshop)} creating with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

            var chatRoom = new ChatRoomWorkshop()
            {
                WorkshopId = workshopId,
                ParentId = parentId,
            };

            try
            {
                return roomRepository.Create(chatRoom);
            }
            catch (DbUpdateException exception)
            {
                logger.LogError($"ChatRoom was not created. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
