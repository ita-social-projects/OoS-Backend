using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service works with repositories for CRUD operations for <see cref = "ChatMessageWorkshop" />.
    /// </summary>
    public class ChatMessageWorkshopService : IChatMessageWorkshopService
    {
        private readonly IEntityRepository<ChatMessageWorkshop> chatMessageRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageWorkshopService"/> class.
        /// </summary>
        /// <param name="chatMessageRepository">Repository for the ChatMessage entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatMessageWorkshopService(IEntityRepository<ChatMessageWorkshop> chatMessageRepository, ILogger logger)
        {
            this.chatMessageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.Debug($"{nameof(ChatMessageWorkshop)} creating was started.");

            try
            {
                var chatMessage = await chatMessageRepository.Create(chatMessageDto.ToDomain()).ConfigureAwait(false);
                logger.Debug($"{nameof(ChatMessageWorkshop)} id:{chatMessage.Id} was saved to DB.");
                return chatMessage.ToModel();
            }
            catch (DbUpdateException exception)
            {
                logger.Error($"{nameof(ChatMessageWorkshop)} was not created. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(long id)
        {
            logger.Debug($"{nameof(ChatMessageWorkshop)} with {nameof(id)}:{id} deleting was started.");

            try
            {
                var chatMessage = (await chatMessageRepository.GetById(id).ConfigureAwait(false))
                    ?? throw new ArgumentOutOfRangeException(nameof(id), $"{nameof(ChatMessageWorkshop)} with id:{id} was not found in the system.");

                await chatMessageRepository.Delete(chatMessage).ConfigureAwait(false);

                logger.Debug($"{nameof(ChatMessageWorkshop)} id:{id} was successfully deleted.");
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Deleting {nameof(ChatMessageWorkshop)} id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageWorkshopDto> GetByIdNoTrackingAsync(long id)
        {
            logger.Debug($"Starting to get the {nameof(ChatMessageWorkshop)} with id:{id}.");

            try
            {
                var chatMessages = await chatMessageRepository.GetByFilterNoTracking(x => x.Id == id).ToListAsync().ConfigureAwait(false);

                return chatMessages.SingleOrDefault()?.ToModel();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting {nameof(ChatMessageWorkshop)} with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageWorkshopDto> UpdateAsync(ChatMessageWorkshopDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.Debug($"{nameof(ChatMessageWorkshop)} with id:{chatMessageDto.Id} updating was started.");

            try
            {
                var chatMessage = await chatMessageRepository.Update(chatMessageDto.ToDomain()).ConfigureAwait(false);

                logger.Debug($"{nameof(ChatMessageWorkshop)} id:{chatMessage.Id} was successfully updated.");

                return chatMessage.ToModel();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Updating {nameof(ChatMessageWorkshop)} with id:{chatMessageDto.Id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(long chatRoomId, OffsetFilter offsetFilter)
        {
            if (offsetFilter is null)
            {
                offsetFilter = new OffsetFilter();
            }

            logger.Debug($"Process of getting a portion of {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} was started.");

            try
            {
                var query = chatMessageRepository.Get<DateTimeOffset>(skip: offsetFilter.From, take: offsetFilter.Size, where: x => x.ChatRoomId == chatRoomId, orderBy: x => x.CreatedDateTime, ascending: false);
                var chatMessages = await query.ToListAsync().ConfigureAwait(false);

                logger.Debug(chatMessages.Count > 0
                    ? $"There are no records in the system with {nameof(chatRoomId)}:{chatRoomId}."
                    : $"Successfully got all {chatMessages.Count} records with {nameof(chatRoomId)}:{chatRoomId}.");

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> UpdateIsReadByCurrentUserInChatRoomAsync(long chatRoomId, bool currentUserRoleIsProvider)
        {
            logger.Debug($"Process of updating {nameof(ChatMessageWorkshop)}s that are not read by current User started.");

            try
            {
                var chatMessages = await chatMessageRepository.Get<long>(where: x => x.ChatRoomId == chatRoomId
                                                                && (x.SenderRoleIsProvider != currentUserRoleIsProvider)
                                                                && x.ReadDateTime == null)
                    .ToListAsync().ConfigureAwait(false);

                if (chatMessages.Count > 0)
                {
                    foreach (var message in chatMessages)
                    {
                        message.ReadDateTime = DateTimeOffset.UtcNow;
                        await chatMessageRepository.Update(message).ConfigureAwait(false);
                    }

                    return chatMessages.Count;
                }

                return default;
            }
            catch (Exception exception)
            {
                logger.Error($"Updating {nameof(ChatMessageWorkshop)}s' status in {nameof(chatRoomId)}:{chatRoomId} and {nameof(currentUserRoleIsProvider)}:{currentUserRoleIsProvider} failed. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
