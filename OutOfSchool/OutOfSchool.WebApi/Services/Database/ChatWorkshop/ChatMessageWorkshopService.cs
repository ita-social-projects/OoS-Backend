﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
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
        private readonly IEntityRepository<ChatMessageWorkshop> messageRepository;
        private readonly IChatRoomWorkshopService roomService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageWorkshopService"/> class.
        /// </summary>
        /// <param name="chatMessageRepository">Repository for the ChatMessage entity.</param>
        /// <param name="roomRepository">Repository for the ChatRoom entity.</param>
        /// <param name="logger">Logger.</param>
        public ChatMessageWorkshopService(IEntityRepository<ChatMessageWorkshop> chatMessageRepository, IChatRoomWorkshopService roomRepository, ILogger logger)
        {
            this.messageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
            this.roomService = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopCreateDto chatMessageCreateDto, Role userRole)
        {
            logger.Debug($"{nameof(ChatMessageWorkshop)} creating was started.");

            if (chatMessageCreateDto is null)
            {
                throw new ArgumentNullException($"{nameof(chatMessageCreateDto)}");
            }

            try
            {
                var userRoleIsProvider = userRole != Role.Parent;

                // find or create new chat room and then set it's Id to the Message model
                var сhatRoomDto = await roomService.CreateOrReturnExistingAsync(chatMessageCreateDto.WorkshopId, chatMessageCreateDto.ParentId).ConfigureAwait(false);

                // create new dto object that will be saved to the database
                var chatMessageDtoThatWillBeSaved = new ChatMessageWorkshop()
                {
                    SenderRoleIsProvider = userRoleIsProvider,
                    Text = chatMessageCreateDto.Text,
                    CreatedDateTime = DateTimeOffset.UtcNow,
                    ReadDateTime = null,
                    ChatRoomId = сhatRoomDto.Id,
                };

                var chatMessage = await messageRepository.Create(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);
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
        public async Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(long chatRoomId, OffsetFilter offsetFilter)
        {
            try
            {
                var chatMessages = await this.GetMessagesForChatRoomDomainModelAsync(chatRoomId, offsetFilter).ConfigureAwait(false);

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all {nameof(ChatMessageWorkshopDto)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(long chatRoomId, OffsetFilter offsetFilter, Role userRole)
        {
            try
            {
                var chatMessages = await this.GetMessagesForChatRoomDomainModelAsync(chatRoomId, offsetFilter).ConfigureAwait(false);

                var userRoleIsProvider = userRole != Role.Parent;
                var notReadChatMessages = chatMessages.Where(x => x.SenderRoleIsProvider != userRoleIsProvider && x.ReadDateTime == null);

                await this.SetReadDateTimeUtcNow(notReadChatMessages).ConfigureAwait(false);

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting and updating all {nameof(ChatMessageWorkshopDto)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        private async Task<List<ChatMessageWorkshop>> GetMessagesForChatRoomDomainModelAsync(long chatRoomId, OffsetFilter offsetFilter)
        {
            if (offsetFilter is null)
            {
                offsetFilter = new OffsetFilter();
            }

            logger.Debug($"Process of getting a portion of {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} was started.");

            try
            {
                var query = messageRepository.Get<DateTimeOffset>(skip: offsetFilter.From, take: offsetFilter.Size, where: x => x.ChatRoomId == chatRoomId, orderBy: x => x.CreatedDateTime, ascending: false);
                var chatMessages = await query.ToListAsync().ConfigureAwait(false);

                logger.Debug(chatMessages.Count > 0
                    ? $"There are no records in the system with {nameof(chatRoomId)}:{chatRoomId}."
                    : $"Successfully got all {chatMessages.Count} records with {nameof(chatRoomId)}:{chatRoomId}.");

                return chatMessages;
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        private async Task<int> SetReadDateTimeUtcNow(IEnumerable<ChatMessageWorkshop> messages)
        {
            if (messages is null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            logger.Debug($"Process of setting {nameof(ChatMessageWorkshop.ReadDateTime)} was started.");

            try
            {
                if (messages.Any())
                {
                    // TODO: implement a new method to save an array of messages
                    foreach (var message in messages)
                    {
                        message.ReadDateTime = DateTimeOffset.UtcNow;
                        await messageRepository.Update(message).ConfigureAwait(false);
                    }

                    logger.Debug($"{messages.Count()} {nameof(messages)} were updated.");
                    return messages.Count();
                }

                return default;
            }
            catch (Exception exception)
            {
                logger.Error($"Updating {nameof(ChatMessageWorkshop.ReadDateTime)} failed. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
