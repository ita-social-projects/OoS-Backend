using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.BlockedProviderParent;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Initializes a new instance of the <see cref="BlockedProviderParentService"/> class.
/// </summary>
/// <param name="blockedProviderParentRepository">Repository for the BlockedProviderParent entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="localizer">Localizer.</param>
/// <param name="mapper">Mapper.</param>
/// <param name="notificationService">Notification service.</param>
/// <param name="parentRepository">Parent repository.</param>
public class BlockedProviderParentService(
    IBlockedProviderParentRepository blockedProviderParentRepository,
    ILogger<BlockedProviderParentService> logger,
    IStringLocalizer<SharedResource> localizer,
    INotificationService notificationService,
    IParentRepository parentRepository) : IBlockedProviderParentService
{
    public const string ProviderIdKey = "ProviderId";
    public const string ProviderFullTitleKey = "ProviderFullTitle";
    public const string ProviderShortTitleKey = "ProviderShortTitle";

    /// <inheritdoc/>
    public async Task<Result<BlockedProviderParentDto>> Block(BlockedProviderParentBlockDto blockedProviderParentBlockDto, string userId)
    {
        logger.LogDebug("BlockedProviderParent blocking was started.");

        if (blockedProviderParentBlockDto == null)
        {
            throw new ArgumentNullException(nameof(blockedProviderParentBlockDto));
        }

        var isBloked = await IsBlocked(blockedProviderParentBlockDto.ParentId, blockedProviderParentBlockDto.ProviderId).ConfigureAwait(false);

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

        var newBlockedProviderParent = blockedProviderParentBlockDto.ToModel();
        newBlockedProviderParent.UserIdBlock = userId;
        newBlockedProviderParent.DateTimeFrom = DateTime.Now;

        var entity = await blockedProviderParentRepository.Block(newBlockedProviderParent).ConfigureAwait(false);

        var blockedParent = await parentRepository.GetById(newBlockedProviderParent.ParentId).ConfigureAwait(false);
        if (blockedParent != null)
        {
            var blockedParentUserId = Guid.Parse(blockedParent.UserId);
            var additionalData = new Dictionary<string, string>()
            {
                { ProviderIdKey, entity.ProviderId.ToString() },
                { ProviderFullTitleKey, entity.Provider.FullTitle },
                { ProviderShortTitleKey, entity.Provider.ShortTitle },
            };

            var recipientsIds = new List<string>() { blockedParentUserId.ToString() };

            await notificationService.Create(
                NotificationType.Parent,
                NotificationAction.ProviderBlock,
                blockedParentUserId,
                recipientsIds,
                additionalData).ConfigureAwait(false);
        }

        return Result<BlockedProviderParentDto>.Success(entity.ToDto());
    }

    /// <inheritdoc/>
    public async Task<Result<BlockedProviderParentDto>> Unblock(BlockedProviderParentUnblockDto blockedProviderParentUnblockDto, string userId)
    {
        logger.LogDebug("BlockedProviderParent unblocking was started.");

        if (blockedProviderParentUnblockDto == null)
        {
            throw new ArgumentNullException(nameof(blockedProviderParentUnblockDto));
        }

        var currentBlock = await blockedProviderParentRepository
            .GetBlockedProviderParentEntities(
                blockedProviderParentUnblockDto.ParentId,
                blockedProviderParentUnblockDto.ProviderId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

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

        var entity = await blockedProviderParentRepository.UnBlock(currentBlock).ConfigureAwait(false);
        var unblockedParentUserId = Guid.Parse(entity.Parent.UserId);
        var additionalData = new Dictionary<string, string>()
            {
                { ProviderIdKey, entity.ProviderId.ToString() },
                { ProviderFullTitleKey, entity.Provider.FullTitle },
                { ProviderShortTitleKey, entity.Provider.ShortTitle },
            };

        var recipientsIds = new List<string>() { unblockedParentUserId.ToString() };

        await notificationService.Create(
            NotificationType.Parent,
            NotificationAction.ProviderUnblock,
            unblockedParentUserId,
            recipientsIds,
            additionalData).ConfigureAwait(false);

        return Result<BlockedProviderParentDto>.Success(entity.ToDto());
    }

    /// <inheritdoc/>
    public async Task<BlockedProviderParentDto> GetBlock(Guid parentId, Guid providerId)
    {
        var currentBlock = await blockedProviderParentRepository
            .GetBlockedProviderParentEntities(parentId, providerId)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        return currentBlock.ToDto();
    }

    public Task<bool> IsBlocked(Guid parentId, Guid providerId)
        => blockedProviderParentRepository
            .GetBlockedProviderParentEntities(parentId, providerId)
            .AnyAsync();
}