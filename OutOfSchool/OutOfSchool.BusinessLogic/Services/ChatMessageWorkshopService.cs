using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ChatWorkshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Service works with repositories for CRUD operations for <see cref = "ChatMessageWorkshop" />.
/// </summary>
public class ChatMessageWorkshopService : IChatMessageWorkshopService
{
    private readonly IEntityRepositorySoftDeleted<Guid, ChatMessageWorkshop> messageRepository;
    private readonly IChatRoomWorkshopService roomService;
    private readonly IHubContext<ChatWorkshopHub> workshopHub;
    private readonly ILogger<ChatMessageWorkshopService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatMessageWorkshopService"/> class.
    /// </summary>
    /// <param name="chatMessageRepository">Repository for the ChatMessage entity.</param>
    /// <param name="roomRepository">Repository for the ChatRoom entity.</param>
    /// <param name="workshopHub">ChatWorkshopHub.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public ChatMessageWorkshopService(
        IEntityRepositorySoftDeleted<Guid, ChatMessageWorkshop> chatMessageRepository,
        IChatRoomWorkshopService roomRepository,
        IHubContext<ChatWorkshopHub> workshopHub,
        ILogger<ChatMessageWorkshopService> logger,
        IMapper mapper)
    {
        this.messageRepository = chatMessageRepository ?? throw new ArgumentNullException(nameof(chatMessageRepository));
        this.roomService = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        this.workshopHub = workshopHub ?? throw new ArgumentNullException(nameof(workshopHub));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<ChatMessageWorkshopDto> CreateAsync(ChatMessageWorkshopCreateDto chatMessageCreateDto, Role userRole)
    {
        logger.LogDebug($"{nameof(ChatMessageWorkshop)} creating was started.");

        if (chatMessageCreateDto is null)
        {
            throw new ArgumentNullException($"{nameof(chatMessageCreateDto)}");
        }

        try
        {
            var userRoleIsProvider = userRole != Role.Parent;

            // create new dto object that will be saved to the database
            var chatMessageDtoThatWillBeSaved = new ChatMessageWorkshop()
            {
                SenderRoleIsProvider = userRoleIsProvider,
                Text = chatMessageCreateDto.Text,
                CreatedDateTime = DateTimeOffset.UtcNow,
                ReadDateTime = null,
                ChatRoomId = chatMessageCreateDto.ChatRoomId,
            };

            var chatMessage = await messageRepository.Create(chatMessageDtoThatWillBeSaved).ConfigureAwait(false);
            logger.LogDebug($"{nameof(ChatMessageWorkshop)} id:{chatMessage.Id} was saved to DB.");
            return mapper.Map<ChatMessageWorkshopDto>(chatMessage);
        }
        catch (ArgumentNullException exception)
        {
            logger.LogError($"{nameof(ChatRoomWorkshopDto)} not exist. Exception: {exception.Message}");
            throw;
        }
        catch (DbUpdateException exception)
        {
            logger.LogError($"{nameof(ChatMessageWorkshop)} was not created. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    [Obsolete("Unused")]
    public async Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAsync(Guid chatRoomId, OffsetFilter offsetFilter)
    {
        try
        {
            var chatMessages = await this.GetMessagesForChatRoomDomainModelAsync(chatRoomId, offsetFilter).ConfigureAwait(false);

            return chatMessages.Select(mapper.Map<ChatMessageWorkshopDto>).ToList();
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatMessageWorkshopDto)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ChatMessageWorkshopDto>> GetMessagesForChatRoomAndSetReadDateTimeIfItIsNullAsync(Guid chatRoomId, OffsetFilter offsetFilter, Role userRole)
    {
        try
        {
            var chatMessages = await this.GetMessagesForChatRoomDomainModelAsync(chatRoomId, offsetFilter).ConfigureAwait(false);

            var userRoleIsProvider = userRole != Role.Parent;
            var notReadChatMessages = chatMessages.Where(x => x.SenderRoleIsProvider != userRoleIsProvider && x.ReadDateTime == null).ToList();

            await this.SetReadDateTimeUtcNow(notReadChatMessages).ConfigureAwait(false);

            if (notReadChatMessages.Count > 0)
            {
                var chatMessageIds = notReadChatMessages.Select(x => x.Id);
                var resultMessage = JsonSerializerHelper.Serialize(chatMessageIds);

                await workshopHub.Clients.Group(chatRoomId.ToString()).SendAsync("ReadChatMessagesByUser", resultMessage).ConfigureAwait(false);
            }

            return chatMessages.Select(mapper.Map<ChatMessageWorkshopDto>).ToList();
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting and updating all {nameof(ChatMessageWorkshopDto)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
            throw;
        }
    }

    private async Task<List<ChatMessageWorkshop>> GetMessagesForChatRoomDomainModelAsync(Guid chatRoomId, OffsetFilter offsetFilter)
    {
        if (offsetFilter is null)
        {
            offsetFilter = new OffsetFilter();
        }

        logger.LogDebug($"Process of getting a portion of {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} was started.");

        try
        {
            var sortExpression = new Dictionary<Expression<Func<ChatMessageWorkshop, object>>, SortDirection>
            {
                { x => x.CreatedDateTime, SortDirection.Descending },
            };

            var query = messageRepository.Get(
                skip: offsetFilter.From,
                take: offsetFilter.Size,
                whereExpression: x => x.ChatRoomId == chatRoomId,
                orderBy: sortExpression);

            var chatMessages = await query.ToListAsync().ConfigureAwait(false);

            logger.LogDebug(chatMessages.Count > 0
                ? $"There are no records in the system with {nameof(chatRoomId)}:{chatRoomId}."
                : $"Successfully got all {chatMessages.Count} records with {nameof(chatRoomId)}:{chatRoomId}.");

            return chatMessages;
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatMessageWorkshop)}s with {nameof(chatRoomId)}:{chatRoomId} failed. Exception: {exception.Message}");
            throw;
        }
    }

    private async Task<int> SetReadDateTimeUtcNow(IEnumerable<ChatMessageWorkshop> messages)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        logger.LogDebug($"Process of setting {nameof(ChatMessageWorkshop.ReadDateTime)} was started.");

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

                logger.LogDebug($"{messages.Count()} {nameof(messages)} were updated.");
                return messages.Count();
            }

            return default;
        }
        catch (Exception exception)
        {
            logger.LogError($"Updating {nameof(ChatMessageWorkshop.ReadDateTime)} failed. Exception: {exception.Message}");
            throw;
        }
    }
}