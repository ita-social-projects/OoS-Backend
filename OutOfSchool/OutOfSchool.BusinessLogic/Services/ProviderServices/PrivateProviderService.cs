using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

/// <summary>
/// Implements interface for functionality for Private Provider.
/// </summary>
public class PrivateProviderService : IPrivateProviderService
{
    private readonly IProviderRepository providerRepository;
    private readonly ILogger<ProviderService> logger;
    private readonly IProviderService providerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivateProviderService"/> class.
    /// </summary>
    /// <param name="providerRepository">Provider repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="providerService">Provider service</param>

    public PrivateProviderService(
        IProviderRepository providerRepository,
        ILogger<ProviderService> logger,
        IProviderService providerService)
    {
        this.providerRepository = providerRepository;
        this.logger = logger;
        this.providerService = providerService;
    }

    public async Task<ProviderLicenseStatusDto> UpdateLicenseStatus(ProviderLicenseStatusDto dto, string userId)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));

        var provider = await providerRepository.GetById(dto.ProviderId).ConfigureAwait(false);

        if (provider is null)
        {
            logger.LogInformation($"Provider(id) {dto.ProviderId} not found. User(id): {userId}");

            return null;
        }

        if (string.IsNullOrEmpty(provider.License) && dto.LicenseStatus != ProviderLicenseStatus.NotProvided)
        {
            logger.LogInformation($"Provider(id) {provider.Id} license is not provided. It cannot be approved. UserId: {userId}");
            throw new ArgumentException("Provider license is not provided. It cannot be approved.");
        }

        if (!string.IsNullOrEmpty(provider.License) && dto.LicenseStatus == ProviderLicenseStatus.NotProvided)
        {
            logger.LogInformation("Cannot set NotProvided license status when license is provided. " +
                                  $"Provider: {provider.Id}. License: {provider.License}. UserId: {userId}");
            throw new ArgumentException("Cannot set NotProvided license status when license is provided.");
        }

        // TODO: validate if current user has permission to update the provider status
        provider.LicenseStatus = dto.LicenseStatus;
        await providerRepository.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation($"Provider(id) {dto.ProviderId} Status was changed to {dto.LicenseStatus}");

        await providerService.SendNotification(provider, NotificationAction.Update, false, true).ConfigureAwait(false);

        return dto;
    }
}
