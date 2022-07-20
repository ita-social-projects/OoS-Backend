using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Address entity.
/// </summary>
public class AddressService : IAddressService
{
    private readonly IEntityRepository<long, Address> repository;
    private readonly ILogger<AddressService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public AddressService(
        IEntityRepository<long, Address> repository,
        ILogger<AddressService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public Task<AddressDto> Create(AddressDto dto)
    {
        logger.LogInformation("Address creating was started.");

        return CreateInternal(mapper.Map<Address>(dto));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AddressDto>> GetAll()
    {
        logger.LogInformation("Getting all Addresses started.");

        var addresses = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(addresses.Any()
            ? $"All {addresses.Count()} records were successfully received from the Address table"
            : "Address table is empty.");

        return mapper.Map<List<AddressDto>>(addresses);
    }

    /// <inheritdoc/>
    public async Task<AddressDto> GetById(long id)
    {
        logger.LogInformation($"Getting Address by Id started. Looking Id = {id}.");

        var address = await repository.GetById(id).ConfigureAwait(false);

        if (address == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got an Address with Id = {id}.");

        return mapper.Map<AddressDto>(address);
    }

    /// <inheritdoc/>
    public async Task<AddressDto> Update(AddressDto dto)
    {
        logger.LogInformation($"Updating Address with Id = {dto?.Id} started.");

        try
        {
            var address = await repository.Update(mapper.Map<Address>(dto)).ConfigureAwait(false);

            logger.LogInformation($"Address with Id = {address?.Id} updated succesfully.");

            return mapper.Map<AddressDto>(address);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. Address with Id = {dto?.Id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Address with Id = {id} started.");

        var entity = new Address { Id = id };

        try
        {
            await repository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation($"Address with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Address with Id = {id} doesn't exist in the system.");
            throw;
        }
    }

    private async Task<AddressDto> CreateInternal(Address address)
    {
        var newAddress = await repository.Create(address).ConfigureAwait(false);

        logger.LogInformation($"Address with Id = {newAddress?.Id} created successfully.");

        return mapper.Map<AddressDto>(newAddress);
    }
}