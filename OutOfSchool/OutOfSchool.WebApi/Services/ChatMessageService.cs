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
    public class ChatMessageService : IChatMessageService
    {
        private readonly IEntityRepository<ChatMessage> repository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the ChatMessage entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatMessageService(IEntityRepository<ChatMessage> repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> Create(ChatMessageDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.Information("ChatMessage creating was started.");

            try
            {
                var chatMessage = await repository.Create(chatMessageDto.ToDomain()).ConfigureAwait(false);
                logger.Information($"ChatMessage id:{chatMessage.Id} was saved to DB.");
                return chatMessage.ToModel();
            }
            catch (DbUpdateException exception)
            {
                logger.Error($"ChatMessage was not created. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"ChatMessage deleting was started. ChatMessage id:{id}");

            try
            {
                var chatMessage = await repository.GetById(id).ConfigureAwait(false);

                if (!(chatMessage is null))
                {
                    await repository.Delete(chatMessage).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(id), $"ChatMessage with id:{id} was not found in the system.");
                }

                logger.Information($"ChatMessage id:{id} was successfully deleted.");
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Deleting ChatMessage id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDto>> GetAllByChatRoomId(long chatRoomId)
        {
            logger.Information($"Process of getting all ChatMessages with ChatRoomId:{chatRoomId} was started.");

            try
            {
                var query = repository.Get<long>(where: x => x.ChatRoomId == chatRoomId, orderBy: x => x.Id);
                var chatMessages = await query.ToListAsync().ConfigureAwait(false);

                logger.Information(!chatMessages.Any()
                ? $"There are no ChatMessages in the system with ChatRoomId:{chatRoomId}"
                : $"Successfully got all {chatMessages.Count} records with ChatRoomId:{chatRoomId}.");

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all ChatMessages with ChatRoomId:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDto>> GetAllNotReadByUserInChatRoom(long chatRoomId, string userId)
        {
            logger.Information($"Process of getting all ChatMessages that are not read with ChatRoomId:{chatRoomId} and UserId:{userId} was started.");

            try
            {
                var query = repository.Get<long>(where: x => x.ChatRoomId == chatRoomId && x.UserId != userId && !x.IsRead, orderBy: x => x.Id);
                var chatMessages = await query.AsNoTracking().ToListAsync().ConfigureAwait(false);

                logger.Information(!chatMessages.Any()
                ? $"There are no ChatMessages that are not read in the system with ChatRoomId:{chatRoomId} and UserId:{userId}."
                : $"Successfully got all {chatMessages.Count} records that are not read with ChatRoomId:{chatRoomId} and UserId:{userId}.");

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all ChatMessages with ChatRoomId:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> GetById(long id)
        {
            logger.Information($"ChatMessage getting was started. ChatMessage id:{id}");

            try
            {
                var chatMessages = await repository.GetByFilterNoTracking(x => x.Id == id).ToListAsync().ConfigureAwait(false);

                if (chatMessages.Count < 1)
                {
                    logger.Information($"ChatMessage id:{id} was not found.");
                    return null;
                }
                else
                {
                    logger.Information($"ChatMessage id:{chatMessages.First().Id} was successfully found.");
                    return chatMessages.First().ToModel();
                }
            }
            catch (Exception exception)
            {
                logger.Error($"Getting ChatMessage with id:{id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDto> Update(ChatMessageDto chatMessageDto)
        {
            if (chatMessageDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageDto)}");
            }

            logger.Information($"ChatMessage updating was started. ChatMessage id:{chatMessageDto.Id}");

            try
            {
                var chatMessage = await repository.Update(chatMessageDto.ToDomain()).ConfigureAwait(false);

                logger.Information($"ChatMessage id:{chatMessage.Id} was successfully updated.");

                return chatMessage.ToModel();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                logger.Error($"Updating ChatMessage with id:{chatMessageDto.Id} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDto>> UpdateIsRead(IEnumerable<ChatMessageDto> chatMessages)
        {
            if (chatMessages is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessages)}");
            }

            logger.Information($"Process of updating ({chatMessages.Count()}) ChatMessages that are not read was started.");

            if (chatMessages.Any())
            {
                foreach (var item in chatMessages)
                {
                    item.IsRead = true;
                    try
                    {
                        await repository.Update(item.ToDomain()).ConfigureAwait(false);
                    }
                    catch (DbUpdateConcurrencyException exception)
                    {
                        logger.Error($"Updating ChatMessage with id:{item.Id} failed. Exception: {exception.Message}");
                        throw;
                    }
                }

                logger.Information($"ChatMessages({chatMessages.Count()}) were successfully updated.");
            }

            return chatMessages;
        }
    }
}
