using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;
namespace OutOfSchool.WebApi.Services;

public class ProviderTypeService:IProviderTypeService
{
    private readonly IEntityRepository<long, ProviderType> repository;
    private readonly ILogger<ProviderTypeService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialGroupService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public ProviderTypeService(
        IEntityRepository<long,
            ProviderType> repository,
        ILogger<ProviderTypeService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProviderTypeDto>> GetAll()
    {
        logger.LogInformation("Getting all Social Groups started.");

        var providerTypes = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!providerTypes.Any()
            ? "SocialGroup table is empty."
            : $"All {providerTypes.Count()} records were successfully received from the SocialGroup table");

        return providerTypes.Select(providerType => mapper.Map<ProviderTypeDto>(providerType)).ToList();
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> GetById(long id)
    {
        logger.LogInformation($"Getting SocialGroup by Id started. Looking Id = {id}.");

        var providerType = await repository.GetById(id).ConfigureAwait(false);

        if (providerType == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a SocialGroup with Id = {id}.");

        return mapper.Map<ProviderTypeDto>(providerType);
    }

    public Task<ProviderTypeDto> Create(ProviderTypeDto dto)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> Create(SocialGroupDto dto)
    {
        logger.LogInformation("SocialGroup creating was started.");

        var providerType = mapper.Map<ProviderType>(dto);

        var newProviderType = await repository.Create(providerType).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {newProviderType?.Id} created successfully.");

        return mapper.Map<ProviderTypeDto>(newProviderType);
    }

    /// <inheritdoc/>
    public async Task<ProviderTypeDto> Update(ProviderTypeDto dto)
    {
        logger.LogInformation($"Updating SocialGroup with Id = {dto?.Id} started.");

        try
        {
            var providerType = await repository.Update(mapper.Map<ProviderType>(dto)).ConfigureAwait(false);

            logger.LogInformation($"SocialGroup with Id = {providerType?.Id} updated succesfully.");

            return mapper.Map<ProviderTypeDto>(providerType);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. SocialGroup with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting SocialGroup with Id = {id} started.");

        var socialGroup = await repository.GetById(id).ConfigureAwait(false);

        if (socialGroup == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"SocialGroup with Id = {id} doesn't exist in the system"]);
        }

        await repository.Delete(socialGroup).ConfigureAwait(false);

        logger.LogInformation($"SocialGroup with Id = {id} succesfully deleted.");
    }
}