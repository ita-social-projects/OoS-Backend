using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

/// <summary>
/// Implements interface for functionality for Public Provider.
/// </summary>
public class PublicProviderService : IPublicProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IProviderService providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProviderService"/> class.
    /// </summary>
    /// <param name="providerRepository">Provider repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="providerService">Provider service</param>

    public PublicProviderService(
        IProviderRepository providerRepository,
        ILogger<ProviderService> logger,
        IProviderService providerService)
    {
        this.providerRepository = providerRepository;
        this.logger = logger;
        this.providerService = providerService;
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
        await providerRepository.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.Status}");

        await providerService.UpdateWorkshopsProviderStatus(dto.ProviderId, dto.Status).ConfigureAwait(false);

        await providerService.SendNotification(provider, NotificationAction.Update, true, false).ConfigureAwait(false);

        return dto;
    }
}