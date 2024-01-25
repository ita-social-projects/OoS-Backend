using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.BlockedProviderParent;

namespace OutOfSchool.WebApi.Services;

public class BlockedProviderParentService : IBlockedProviderParentService, INotificationReciever
{
    private readonly IBlockedProviderParentRepository blockedProviderParentRepository;
    private readonly ILogger<BlockedProviderParentService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly INotificationService notificationService;
    private readonly IParentRepository parentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockedProviderParentService"/> class.
    /// </summary>
    /// <param name="blockedProviderParentRepository">Repository for the BlockedProviderParent entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="notificationService">Notification service.</param>
    /// <param name="parentRepository">Parent repository.</param>
    public BlockedProviderParentService(
        IBlockedProviderParentRepository blockedProviderParentRepository,
        ILogger<BlockedProviderParentService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        INotificationService notificationService,
        IParentRepository parentRepository)
    {
        this.blockedProviderParentRepository = blockedProviderParentRepository;
        this.logger = logger;
        this.localizer = localizer;
        this.mapper = mapper;
        this.notificationService = notificationService;
        this.parentRepository = parentRepository;
    }

    /// <inheritdoc/>
    public async Task<Result<BlockedProviderParentDto>> Block(BlockedProviderParentBlockDto blockedProviderParentBlockDto, string userId)
    {
        logger.LogDebug("BlockedProviderParent blocking was started.");

        if (blockedProviderParentBlockDto == null)
        {
            throw new ArgumentNullException(nameof(blockedProviderParentBlockDto));
        }

        bool isBloked = await IsBlocked(blockedProviderParentBlockDto.ParentId, blockedProviderParentBlockDto.ProviderId).ConfigureAwait(false);

        if (isBloked)
        {
            logger.LogError($"Block exists for ParentId: {blockedProviderParentBlockDto.ParentId}, ProviderId: {blockedProviderParentBlockDto.ProviderId}.");
            return Result<BlockedProviderParentDto>.Failed(new OperationError
            {
                Code = "400",
                Description = localizer[
                    "Block exists for ParentId: {0}, ProviderId: {1}.",
                    blockedProviderParentBlockDto.ParentId,
                    blockedProviderParentBlockDto.ProviderId],
            });
        }

        var newBlockedProviderParent = mapper.Map<BlockedProviderParent>(blockedProviderParentBlockDto);
        newBlockedProviderParent.UserIdBlock = userId;
        newBlockedProviderParent.DateTimeFrom = DateTime.Now;

        var entity = await blockedProviderParentRepository.Block(newBlockedProviderParent).ConfigureAwait(false);

        var blockedParent = await parentRepository.GetById(newBlockedProviderParent.ParentId).ConfigureAwait(false);
        if (blockedParent != null)
        {
            var blockedParentUserId = Guid.Parse(blockedParent.UserId);
            await notificationService.Create(
                NotificationType.Parent,
                NotificationAction.Block,
                blockedParentUserId,
                this).ConfigureAwait(false);
        }

        return Result<BlockedProviderParentDto>.Success(mapper.Map<BlockedProviderParentDto>(entity));
    }

    /// <inheritdoc/>
    public async Task<Result<BlockedProviderParentDto>> Unblock(BlockedProviderParentUnblockDto blockedProviderParentUnblockDto, string userId)
    {
        logger.LogDebug("BlockedProviderParent unblocking was started.");

        if (blockedProviderParentUnblockDto == null)
        {
            throw new ArgumentNullException(nameof(blockedProviderParentUnblockDto));
        }

        var currentBlock = await GetBlock(blockedProviderParentUnblockDto.ParentId, blockedProviderParentUnblockDto.ProviderId).ConfigureAwait(false);

        if (currentBlock is null)
        {
            logger.LogError($"Block does not exist for ParentId: {blockedProviderParentUnblockDto.ParentId}, ProviderId: {blockedProviderParentUnblockDto.ProviderId}.");
            return Result<BlockedProviderParentDto>.Failed(new OperationError
            {
                Code = "400",
                Description = localizer[
                    "Block does not exist for ParentId: {0}, ProviderId: {1}.",
                    blockedProviderParentUnblockDto.ParentId,
                    blockedProviderParentUnblockDto.ProviderId],
            });
        }

        currentBlock.DateTimeTo = DateTime.Now;
        currentBlock.UserIdUnblock = userId;

        var entity = await blockedProviderParentRepository.UnBlock(mapper.Map<BlockedProviderParent>(currentBlock)).ConfigureAwait(false);

        return Result<BlockedProviderParentDto>.Success(mapper.Map<BlockedProviderParentDto>(entity));
    }

    /// <inheritdoc/>
    public async Task<BlockedProviderParentDto> GetBlock(Guid parentId, Guid providerId)
    {
        var currentBlock = await blockedProviderParentRepository.GetByFilter(
            b => b.ParentId == parentId
                 && b.ProviderId == providerId
                 && b.DateTimeTo == null).ConfigureAwait(false);

        return mapper.Map<BlockedProviderParentDto>(currentBlock.FirstOrDefault());
    }

    public async Task<bool> IsBlocked(Guid parentId, Guid providerId)
    {
        var currentBlock = await blockedProviderParentRepository.GetByFilter(
            b => b.ParentId == parentId
                 && b.ProviderId == providerId
                 && b.DateTimeTo == null).ConfigureAwait(false);

        return currentBlock.Any();
    }

    public Task<IEnumerable<string>> GetNotificationsRecipientIds(
        NotificationAction action,
        Dictionary<string, string> additionalData,
        Guid objectId)
    {
        return Task.FromResult<IEnumerable<string>>(new List<string>() { objectId.ToString() });
    }
}