using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressService"/> class.
    /// </summary>
    /// <param name="repository">Repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    public AddressService(
        IEntityRepository<long, Address> repository,
        ILogger<AddressService> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        this.localizer = localizer;
        this.repository = repository;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public Task<AddressDto> Create(AddressDto dto)
    {
        logger.LogInformation("Address creating was started.");

        return CreateInternal(dto.ToDomain());
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AddressDto>> GetAll()
    {
        logger.LogInformation("Getting all Addresses started.");

        var addresses = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!addresses.Any()
            ? "Address table is empty."
            : $"All {addresses.Count()} records were successfully received from the Address table");

        return addresses.Select(address => address.ToModel()).ToList();
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

        return address.ToModel();
    }

    /// <inheritdoc/>
    public async Task<AddressDto> Update(AddressDto dto)
    {
        logger.LogInformation($"Updating Address with Id = {dto?.Id} started.");

        try
        {
            var address = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

            logger.LogInformation($"Address with Id = {address?.Id} updated succesfully.");

            return address.ToModel();
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

        return newAddress.ToModel();
    }
}