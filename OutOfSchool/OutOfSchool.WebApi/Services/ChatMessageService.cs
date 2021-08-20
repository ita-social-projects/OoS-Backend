using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service works with repositories for CRUD operations for <see cref = "ChatMessage" />.
    /// </summary>
    public class ChatMessageService : IChatMessageService
    {
        private readonly IEntityRepository<ChatMessage> repository;
        private readonly ILogger<ChatMessageService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the ChatMessage entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatMessageService(IEntityRepository<ChatMessage> repository, ILogger<ChatMessageService> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> CreateAsync(ChatMessageDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.LogInformation($"{nameof(ChatMessage)} creating was started.");

            try
            {
                var chatMessage = await repository.Create(chatMessageDto.ToDomain()).ConfigureAwait(false);
                logger.LogInformation($"{nameof(ChatMessage)} id:{chatMessage.Id} was saved to DB.");
                return chatMessage.ToModel();
            }
            catch (DbUpdateException exception)
            {
                logger.LogError($"{nameof(ChatMessage)} was not created. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(long id)
        {
            logger.LogInformation($"{nameof(ChatMessage)} with {nameof(id)}:{id} deleting was started.");

            try
            {
                var chatMessage = await repository.GetById(id).ConfigureAwait(false);

                if (!(chatMessage is null))
                {
                    await repository.Delete(chatMessage).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(id), $"{nameof(ChatMessage)} with id:{id} was not found in the system.");
                }

                logger.LogInformation($"{nameof(ChatMessage)} id:{id} was successfully deleted.");
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Deleting {nameof(ChatMessage)} id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> GetByIdNoTrackingAsync(long id)
        {
            logger.LogInformation($"Starting to get the {nameof(ChatMessage)} with id:{id}.");

            try
            {
                var chatMessages = await repository.GetByFilterNoTracking(x => x.Id == id).ToListAsync().ConfigureAwait(false);

                return chatMessages.SingleOrDefault()?.ToModel();
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting {nameof(ChatMessage)} with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> UpdateAsync(ChatMessageDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.LogInformation($"{nameof(ChatMessage)} with id:{chatMessageDto.Id} updating was started.");

            try
            {
                var chatMessage = await repository.Update(chatMessageDto.ToDomain()).ConfigureAwait(false);

                logger.LogInformation($"{nameof(ChatMessage)} id:{chatMessage.Id} was successfully updated.");

                return chatMessage.ToModel();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.LogError($"Updating {nameof(ChatMessage)} with id:{chatMessageDto.Id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDto>> GetMessagesForChatRoomAsync(long chatRoomId, OffsetFilter offsetFilter)
        {
            if (offsetFilter is null)
            {
                offsetFilter = new OffsetFilter() { From = 0, Size = 20 };
            }

            logger.LogInformation($"Process of getting a portion of {nameof(ChatMessage)}s with {nameof(chatRoomId)}:{chatRoomId} was started.");

            try
            {
                var query = repository.Get<DateTimeOffset>(skip: offsetFilter.From, take: offsetFilter.Size, where: x => x.ChatRoomId == chatRoomId, orderBy: x => x.CreatedTime, ascending: false);
                var chatMessages = await query.ToListAsync().ConfigureAwait(false);

                logger.LogInformation(!chatMessages.Any()
                ? $"There are no records in the system with {nameof(chatRoomId)}:{chatRoomId}."
                : $"Successfully got all {chatMessages.Count} records with {nameof(chatRoomId)}:{chatRoomId}.");

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.LogError($"Getting all {nameof(ChatMessage)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> UpdateIsReadByCurrentUserInChatRoomAsync(long chatRoomId, bool currentUserRoleIsProvider)
        {
            logger.LogInformation($"Process of updating {nameof(ChatMessage)}s that are not read by current User started.");

            try
            {
                var chatMessages = await repository.Get<long>(where: x => x.ChatRoomId == chatRoomId
                                                                && (x.SenderRoleIsProvider != currentUserRoleIsProvider)
                                                                && !x.IsRead)
                    .ToListAsync().ConfigureAwait(false);

                if (chatMessages.Count > 0)
                {
                    foreach (var message in chatMessages)
                    {
                        message.IsRead = true;
                        await repository.Update(message).ConfigureAwait(false);
                    }

                    return chatMessages.Count;
                }

                return default;
            }
            catch (Exception exception)
            {
                logger.LogError($"Updating {nameof(ChatMessage)}s' status in {nameof(chatRoomId)}:{chatRoomId} and {nameof(currentUserRoleIsProvider)}:{currentUserRoleIsProvider} failed. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
