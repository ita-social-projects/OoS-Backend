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
        public async Task<ChatMessageDTO> Create(ChatMessageDTO chatMessageDto)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("ChatMessage creating was started. ChatMessageDTO:");
            strBuilder.AppendLine($"chatMessageDto.Id: {chatMessageDto.Id}");
            strBuilder.AppendLine($"chatMessageDto.UserId: {chatMessageDto.UserId}");
            strBuilder.AppendLine($"chatMessageDto.ChatRoomId: {chatMessageDto.ChatRoomId}");
            strBuilder.AppendLine($"chatMessageDto.Text: {chatMessageDto.Text}");
            strBuilder.AppendLine($"chatMessageDto.CreatedTime: {chatMessageDto.CreatedTime}");
            strBuilder.AppendLine($"chatMessageDto.Read: {chatMessageDto.IsRead}");
            logger.Information(strBuilder.ToString());

            try
            {
                var chatMessage = await repository.Create(chatMessageDto.ToDomain()).ConfigureAwait(false);
                logger.Information($"ChatMessage id:{chatMessage.Id} was saved to DB.");
                return chatMessage.ToModel();
            }
            catch (DbUpdateException ex)
            {
                logger.Error($"ChatMessage was not created. Exception: {ex.Message}");
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
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"Deleting ChatMessage id:{id} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDTO>> GetAllByChatRoomId(long chatRoomId)
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
            catch (Exception ex)
            {
                logger.Error($"Getting all ChatMessages with ChatRoomId:{chatRoomId} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDTO>> GetAllNotReadByUserInChatRoom(long chatRoomId, string userId)
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
            catch (Exception ex)
            {
                logger.Error($"Getting all ChatMessages with ChatRoomId:{chatRoomId} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDTO> GetById(long id)
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
            catch (Exception ex)
            {
                logger.Error($"Getting ChatMessage with id:{id} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatMessageDTO> Update(ChatMessageDTO chatMessageDto)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("ChatMessage Updating was started. ChatMessageDTO:");
            strBuilder.AppendLine($"chatMessageDto.Id: {chatMessageDto.Id}");
            strBuilder.AppendLine($"chatMessageDto.UserId: {chatMessageDto.UserId}");
            strBuilder.AppendLine($"chatMessageDto.ChatRoomId: {chatMessageDto.ChatRoomId}");
            strBuilder.AppendLine($"chatMessageDto.Text: {chatMessageDto.Text}");
            strBuilder.AppendLine($"chatMessageDto.CreatedTime: {chatMessageDto.CreatedTime}");
            strBuilder.AppendLine($"chatMessageDto.Read: {chatMessageDto.IsRead}");
            logger.Information(strBuilder.ToString());

            try
            {
                var chatMessage = await repository.Update(chatMessageDto.ToDomain()).ConfigureAwait(false);

                logger.Information($"ChatMessage id:{chatMessage.Id} was successfully updated.");

                return chatMessage.ToModel();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"Updating ChatMessage with id:{chatMessageDto.Id} failed. Exception: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChatMessageDTO>> UpdateIsRead(IEnumerable<ChatMessageDTO> chatMessages)
        {
            logger.Information($"Process of updating ({chatMessages.Count()}) ChatMessages that are not read was started.");

            if (chatMessages.Any())
            {
                foreach (var item in chatMessages)
                {
                    item.IsRead = true;
                    try
                    {
                        var result = await repository.Update(item.ToDomain()).ConfigureAwait(false);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        logger.Error($"Updating ChatMessage with id:{item.Id} failed. Exception: {ex.Message}");
                        throw;
                    }
                }

                logger.Information($"ChatMessages({chatMessages.Count()}) were successfully updated.");
            }
            
            return chatMessages;
        }
    }
}
