using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Provider entity.
    /// </summary>
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="repository">Repository for some entity.</param>
        public ProviderService(IProviderRepository repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public Task<ProviderDto> Create(ProviderDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Provider was null.");
            }

            var provider = dto.ToDomain();

            if (repository.IsAlreadyExisted(provider))
            {
                throw new ArgumentException("There is already an providerDto with such data");
            }

            return CreateInternal(provider);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProviderDto>> GetAll()
        {
             var providers = await repository.GetAll().ConfigureAwait(false);

             return providers.Select(organization => organization.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetById(long id)
        {
            var provider = await repository.GetById(id).ConfigureAwait(false);
            if (provider == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return provider.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Update(ProviderDto dto)
        {
            try
            {
                var provider = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                return provider.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(dto)} could not be updated: {ex.Message}");
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
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }

        private async Task<ProviderDto> CreateInternal(Provider provider)
        {
            var newProvider = await repository.Create(provider).ConfigureAwait(false);

            return newProvider.ToModel();
        }
    }
}