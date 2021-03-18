using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Address entity.
    /// </summary>
    public class AddressService : IAddressService
    {
        private readonly IEntityRepository<Address> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Address entity.</param>
        public AddressService(IEntityRepository<Address> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public Task<AddressDto> Create(AddressDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Address is null.");
            }

            var address = dto.ToDomain();

            return CreateInternal(address);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AddressDto>> GetAll()
        {
            var addresses = await repository.GetAll().ConfigureAwait(false);

            return addresses.Select(address => address.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<AddressDto> GetById(long id)
        {
            var address = await repository.GetById(id).ConfigureAwait(false);

            if (address == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return address.ToModel();
        }

        /// <inheritdoc/>
        public async Task<AddressDto> Update(AddressDto dto)
        {
            try
            {
                var address = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return address.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            try
            {
                await repository
                    .Delete(await repository.GetById(id).ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        private async Task<AddressDto> CreateInternal(Address address)
        {
            var newAddress = await repository.Create(address).ConfigureAwait(false);

            return newAddress.ToModel();
        }
    }
}
