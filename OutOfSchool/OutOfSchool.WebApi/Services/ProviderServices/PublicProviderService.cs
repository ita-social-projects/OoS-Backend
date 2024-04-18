using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services.ProviderServices;

/// <summary>
/// Implements interface for functionality for Public Provider.
/// </summary>
public class PublicProviderService : IPublicProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IProviderService providerService;
    private readonly IChangesLogService changesLogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProviderService"/> class.
    /// </summary>
    /// <param name="providerRepository">Provider repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="providerService">Provider service</param>

    public PublicProviderService(
        IProviderRepository providerRepository,
        ILogger<ProviderService> logger,
        IProviderService providerService,
        IChangesLogService changesLogService)
    {
        this.providerRepository = providerRepository;
        this.logger = logger;
        this.providerService = providerService;
        this.changesLogService = changesLogService;
    }

    public async Task<ProviderStatusDto> UpdateStatus(ProviderStatusDto dto, string userId)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var provider = await providerRepository.GetById(dto.ProviderId).ConfigureAwait(false);

        if (provider is null)
        {
            logger.LogInformation($"Provider(id) {dto.ProviderId} not found. User(id): {userId}");

            return null;
        }

        // TODO: validate if current user has permission to update the provider status
        provider.Status = dto.Status;
        provider.StatusReason = dto.StatusReason;

        changesLogService.AddEntityChangesToDbContext(provider, userId);

        await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.Status}");

        await providerService.UpdateWorkshopsProviderStatus(dto.ProviderId, dto.Status).ConfigureAwait(false);

        await providerService.SendNotification(provider, NotificationAction.Update, true, false).ConfigureAwait(false);

        return dto;
    }
}