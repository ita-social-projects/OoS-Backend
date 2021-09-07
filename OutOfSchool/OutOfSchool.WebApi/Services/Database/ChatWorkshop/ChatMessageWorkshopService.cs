using System;
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

                return chatMessages.Select(item => item.ToModel()).ToList();
            }
            catch (Exception exception)
            {
                logger.Error($"Getting all {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> SetReadDatetimeInAllMessagesForUserInChatRoomAsync(long chatRoomId, Role userRole)
        {
            logger.Debug($"Process of updating {nameof(ChatMessageWorkshop)}s that are not read by current User started.");

            try
            {
                var userRoleIsProvider = userRole != Role.Parent;

                var chatMessages = await messageRepository.Get<long>(where: x => x.ChatRoomId == chatRoomId
                                                                && (x.SenderRoleIsProvider != userRoleIsProvider)
                                                                && x.ReadDateTime == null)
                    .ToListAsync().ConfigureAwait(false);

                if (chatMessages.Count > 0)
                {
                    foreach (var message in chatMessages)
                    {
                        message.ReadDateTime = DateTimeOffset.UtcNow;
                        await messageRepository.Update(message).ConfigureAwait(false);
                    }

                    return chatMessages.Count;
                }

                return default;
            }
            catch (Exception exception)
            {
                logger.Error($"Updating {nameof(ChatMessageWorkshop)}s' status in {nameof(chatRoomId)}:{chatRoomId} and {nameof(userRole)}:{userRole} failed. Exception: {exception.Message}");
                throw;
            }
        }
    }
}
