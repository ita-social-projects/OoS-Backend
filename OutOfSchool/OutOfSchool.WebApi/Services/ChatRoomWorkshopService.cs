using AutoMapper;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;
using System.Linq.Expressions;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Service works with repositories for CRUD operations for <see cref = "ChatRoomWorkshop" />.
/// </summary>
public class ChatRoomWorkshopService : IChatRoomWorkshopService
{
    private readonly IEntityRepository<Guid, ChatRoomWorkshop> roomRepository;
    private readonly IChatRoomWorkshopModelForChatListRepository roomWorkshopWithLastMessageRepository;
    private readonly ILogger<ChatRoomWorkshopService> logger;
    private readonly IMapper mapper;
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatRoomWorkshopService"/> class.
    /// </summary>
    /// <param name="chatRoomRepository">Repository for the ChatRoom entity.</param>
    /// <param name="roomWorkshopWithLastMessageRepository">Repository for the ChatRoom entity with special model.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public ChatRoomWorkshopService(
        IEntityRepository<Guid, ChatRoomWorkshop> chatRoomRepository,
        ILogger<ChatRoomWorkshopService> logger,
        IChatRoomWorkshopModelForChatListRepository roomWorkshopWithLastMessageRepository,
        IMapper mapper)
    {
        this.roomRepository = chatRoomRepository ?? throw new ArgumentNullException(nameof(chatRoomRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.roomWorkshopWithLastMessageRepository = roomWorkshopWithLastMessageRepository ?? throw new ArgumentNullException(nameof(roomWorkshopWithLastMessageRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<ChatRoomWorkshopDto> CreateOrReturnExistingAsync(Guid workshopId, Guid parentId)
    {
        logger.LogDebug($"Checking a {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

        try
        {
            var existingChatRoom = await this.GetUniqueChatRoomAsync(workshopId, parentId).ConfigureAwait(false);

            if (existingChatRoom is null)
            {
                var newChatRoom = await this.CreateAsync(workshopId, parentId).ConfigureAwait(false);
                logger.LogDebug($"{nameof(ChatRoomWorkshop)} id:{newChatRoom.Id} was saved to DB.");
                return mapper.Map<ChatRoomWorkshopDto>(newChatRoom);
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
    public async Task DeleteAsync(Guid id)
    {
        logger.LogDebug($"{nameof(ChatRoomWorkshop)} {nameof(id)}:{id} deleting was started.");

        try
        {
            var query = roomRepository.Get(includeProperties: $"{nameof(ChatRoomWorkshop.ChatMessages)}", where: x => x.Id == id);
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
    public async Task<ChatRoomWorkshopDto> GetByIdAsync(Guid id)
    {
        logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} by Id:{id} was started.");

        try
        {
            var chatRooms = await roomRepository.GetByFilter(
                    predicate: x => x.Id == id,
                    includeProperties: $"{nameof(ChatRoomWorkshop.Parent)},{nameof(ChatRoomWorkshop.Workshop)}")
                .ConfigureAwait(false);

            var chatRoom = chatRooms.SingleOrDefault();

            return chatRoom is null ? null : mapper.Map<ChatRoomWorkshopDto>(chatRoom);
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting {nameof(ChatRoomWorkshop)} with id:{id} failed. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChatRoomWorkshopDto>> GetByParentIdProviderIdAsync(Guid parentId, Guid providerId)
    {
        logger.LogDebug("Process of getting ChatRooms with parentId:{parentId} and providerId:{providerId} was started.", parentId, providerId);

        try
        {
            var rooms = (await roomRepository.GetByFilter(
                predicate: x => x.ParentId == parentId && x.Workshop.ProviderId == providerId)
                .ConfigureAwait(false)).Select(x => mapper.Map<ChatRoomWorkshopDto>(x)).ToList();

            if (rooms.Count > 0)
            {
                logger.LogDebug("There is no Chat rooms in the system with parentId:{parentId} and providerId:{providerId}.", parentId, providerId);
            }
            else
            {
                logger.LogDebug("Successfully got all {roomsCount} records with parentId:{parentId} and providerId:{providerId}.", rooms.Count, parentId, providerId);
            }

            return rooms;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Getting all ChatRooms with parentId:{parentId} and providerId:{providerId}. Exception: {exception}",
                parentId,
                providerId,
                exception.Message);

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ChatRoomWorkshopDto> GetByParentIdWorkshopIdAsync(Guid parentId, Guid workshopId)
    {
        logger.LogDebug("Process of getting ChatRoom with parentId:{parentId} and workshopId:{workshopId} was started.", parentId, workshopId);

        try
        {
            var room = (await roomRepository.GetByFilter(
                predicate: x => x.ParentId == parentId && x.WorkshopId == workshopId)
                .ConfigureAwait(false)).SingleOrDefault();

            if (room is null)
            {
                logger.LogDebug("There is no Chat rooms in the system with parentId:{parentId} and workshopId:{workshopId}.", parentId, workshopId);
            }
            else
            {
                logger.LogDebug("Successfully got record with parentId:{parentId} and workshopId:{workshopId}.", parentId, workshopId);
            }

            return mapper.Map<ChatRoomWorkshopDto>(room);
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Getting ChatRoom with parentId:{parentId} and workshopId:{workshopId}. Exception: {exception}",
                parentId,
                workshopId,
                exception.Message);

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByParentIdAsync(Guid parentId)
    {
        logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(parentId)}:{parentId} was started.");

        try
        {
            var rooms = await roomWorkshopWithLastMessageRepository.GetByParentIdAsync(parentId).ConfigureAwait(false);
            logger.LogDebug(rooms.Count > 0
                ? $"There is no Chat rooms in the system with userId:{parentId}."
                : $"Successfully got all {rooms.Count} records with userId:{parentId}.");
            return rooms.Select(x => mapper.Map<ChatRoomWorkshopDtoWithLastMessage>(x));
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(parentId)}:{parentId}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByProviderIdAsync(Guid providerId)
    {
        logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(providerId)}:{providerId} was started.");

        try
        {
            var rooms = await roomWorkshopWithLastMessageRepository.GetByProviderIdAsync(providerId).ConfigureAwait(false);
            logger.LogDebug(rooms.Count > 0
                ? $"There is no Chat rooms in the system with userId:{providerId}."
                : $"Successfully got all {rooms.Count} records with userId:{providerId}.");
            return rooms.Select(x => mapper.Map<ChatRoomWorkshopDtoWithLastMessage>(x));
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(providerId)}:{providerId}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdAsync(Guid workshopId)
    {
        logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(workshopId)}:{workshopId} was started.");

        try
        {
            var rooms = await roomWorkshopWithLastMessageRepository.GetByWorkshopIdAsync(workshopId).ConfigureAwait(false);
            logger.LogDebug(rooms.Count > 0
                ? $"There is no Chat rooms in the system with userId:{workshopId}."
                : $"Successfully got all {rooms.Count} records with userId:{workshopId}.");
            return rooms.Select(x => mapper.Map<ChatRoomWorkshopDtoWithLastMessage>(x));
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {nameof(workshopId)}:{workshopId}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ChatRoomWorkshopDtoWithLastMessage>> GetByWorkshopIdsAsync(IEnumerable<Guid> workshopIds)
    {
        string workshopIdsStr = $"{nameof(workshopIds)}:{string.Join(", ", workshopIds)}";
        logger.LogDebug($"Process of getting  {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {workshopIdsStr} was started.");

        try
        {
            var rooms = await roomWorkshopWithLastMessageRepository.GetByWorkshopIdsAsync(workshopIds).ConfigureAwait(false);

            logger.LogDebug(rooms.Count > 0
                ? $"There is no Chat rooms in the system with userId:{workshopIdsStr}."
                : $"Successfully got all {rooms.Count} records with userId:{workshopIdsStr}.");

            return rooms.Select(x => mapper.Map<ChatRoomWorkshopDtoWithLastMessage>(x));
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatRoomWorkshopDtoWithLastMessage)}(s/es) with {workshopIdsStr}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetChatRoomIdsByParentIdAsync(Guid parentId)
    {
        logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} WorkshopIds with {nameof(parentId)}:{parentId} was started.");

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
            logger.LogError($"Getting all {nameof(ChatRoomWorkshop)} WorkshopIds with {nameof(parentId)}:{parentId}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetChatRoomIdsByProviderIdAsync(Guid providerId)
    {
        logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} WorkshopIds with {nameof(providerId)}:{providerId} was started.");

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
            logger.LogError($"Getting all {nameof(ChatRoomWorkshop)} WorkshopIds with {nameof(providerId)}:{providerId}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Guid>> GetChatRoomIdsByWorkshopIdsAsync(IEnumerable<Guid> workshopIds)
    {
        string workshopIdsStr = $"{nameof(workshopIds)}:{string.Join(", ", workshopIds)}";
        logger.LogDebug($"Process of getting {nameof(ChatRoomWorkshop)} WorkshopIds with {workshopIdsStr} was started.");

        try
        {
            var rooms = await roomRepository.GetByFilter(r => workshopIds.Contains(r.WorkshopId)).ConfigureAwait(false);
            logger.LogDebug(!rooms.Any()
                ? $"There is no Chat rooms in the system with workshopIds:{workshopIdsStr}."
                : $"Successfully got all {rooms.Count()} records with workshopIds:{workshopIdsStr}.");
            return rooms.Select(x => x.Id);
        }
        catch (Exception exception)
        {
            logger.LogError($"Getting all {nameof(ChatRoomWorkshop)} WorkshopIds with {workshopIdsStr}. Exception: {exception.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ChatRoomWorkshopDto> GetUniqueChatRoomAsync(Guid workshopId, Guid parentId)
    {
        logger.LogDebug($"Process of getting unique {nameof(ChatRoomWorkshop)} with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId} was started.");

        try
        {
            var chatRooms = await roomRepository.GetByFilter(r => r.WorkshopId == workshopId && r.ParentId == parentId, $"{nameof(ChatRoomWorkshop.Parent)},{nameof(ChatRoomWorkshop.Workshop)}").ConfigureAwait(false);
            var chatRoom = chatRooms.SingleOrDefault();

            logger.LogDebug(chatRoom is null
                ? $"There is no {nameof(ChatRoomWorkshop)} in the system with {nameof(workshopId)}:{workshopId} and {nameof(parentId)}:{parentId}."
                : $"Successfully got a {nameof(ChatRoomWorkshop)} with {nameof(chatRoom.Id)}:{chatRoom.Id}.");

            return chatRoom is null ? null : mapper.Map<ChatRoomWorkshopDto>(chatRoom);
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
    private Task<ChatRoomWorkshop> CreateAsync(Guid workshopId, Guid parentId)
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
    public async Task<SearchResult<ChatRoomWorkshopDtoWithLastMessage>> GetChatRoomByFilter(ChatWorkshopFilter filter, Guid userId)
    {
        logger.LogInformation("Getting ChatRoomWorkshops by filter started.");

        filter ??= new ChatWorkshopFilter();

        var filterPredicate = PredicateBuild(filter, userId);

        var rooms = roomRepository.Get(
                where: filterPredicate);

        var roomsCount = rooms.Count();
        var roomsList = rooms.Skip(filter.From).Take(filter.Size).ToList();

        var chatRoomsWithMessages = await roomWorkshopWithLastMessageRepository
            .GetByWorkshopIdsAsync(roomsList.Select(x => x.WorkshopId));

        logger.LogInformation(!rooms.Any()
            ? "There was no matching entity found."
            : $"All matching {roomsCount} records were successfully received from the ChatWorkshop table");

        var results = chatRoomsWithMessages.Select(x => mapper.Map<ChatRoomWorkshopDtoWithLastMessage>(x)).ToList();

        return new SearchResult<ChatRoomWorkshopDtoWithLastMessage>()
        {
            Entities = results,
            TotalAmount = roomsCount,
        };
    }

    private Expression<Func<ChatRoomWorkshop, bool>> PredicateBuild(ChatWorkshopFilter filter, Guid userId)
    {
        var predicate = PredicateBuilder.True<ChatRoomWorkshop>();

        if (userId != default)
        {
            predicate = predicate.And(x => x.ParentId == userId || x.Workshop.ProviderId == userId);
        }

        if (filter.WorkshopIds is not null && filter.WorkshopIds.Any())
        {
            predicate = predicate.And(x => filter.WorkshopIds.Any(c => c == x.WorkshopId));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var tempPredicate = PredicateBuilder.False<ChatRoomWorkshop>()
                .Or(x => x.Workshop.Provider.User.LastName.ToLower().StartsWith(filter.SearchText.ToLower()))
                .Or(x => x.Parent.User.LastName.ToLower().StartsWith(filter.SearchText.ToLower()))
                .Or(x => x.Workshop.Provider.User.FirstName.ToLower().StartsWith(filter.SearchText.ToLower()))
                .Or(x => x.Parent.User.FirstName.ToLower().StartsWith(filter.SearchText.ToLower()))
                .Or(x => x.Workshop.Provider.User.Email.StartsWith(filter.SearchText))
                .Or(x => x.Parent.User.Email.StartsWith(filter.SearchText))
                .Or(x => x.Workshop.Title.ToLower().Contains(filter.SearchText.ToLower()))
                .Or(x => x.Parent.User.PhoneNumber.StartsWith(filter.SearchText))
                .Or(x => x.Workshop.Provider.PhoneNumber.StartsWith(filter.SearchText));

            predicate = predicate.And(tempPredicate);
        }

        return predicate;
    }
}