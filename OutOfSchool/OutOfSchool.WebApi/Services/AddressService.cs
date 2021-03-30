using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Address entity.
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IEntityRepository<Address> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public AddressService(IEntityRepository<Address> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<AddressDto> Create(AddressDto dto)
        {
            logger.Information("Address creating was started.");

            return CreateInternal(dto.ToDomain());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AddressDto>> GetAll()
        {
            logger.Information("Process of getting all Addresses started.");

            var addresses = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!addresses.Any()
                ? "Address table is empty."
                : "Successfully got all records from the Address table.");

            return addresses.Select(address => address.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<AddressDto> GetById(long id)
        {
            logger.Information("Process of getting Adress by id started.");

            var address = await repository.GetById(id).ConfigureAwait(false);

            if (address == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got an Address with id = {id}.");

            return address.ToModel();
        }

        /// <inheritdoc/>
        public async Task<AddressDto> Update(AddressDto dto)
        {
            logger.Information("Address updating was launched.");

            try
            {
                var address = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Address successfully updated.");

                return address.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Address in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Address deleting was launched.");

            var entity = new Address { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.Information("Address successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Address in the Db with such an id.");
                throw;
            }
        }

        private async Task<AddressDto> CreateInternal(Address address)
        {
            var newAddress = await repository.Create(address).ConfigureAwait(false);

            logger.Information("Address created successfully.");

            return newAddress.ToModel();
        }
    }
}
