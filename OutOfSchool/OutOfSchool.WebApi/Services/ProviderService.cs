using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private IProviderRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="repository">Repository for some entity.</param>
        public ProviderService(IProviderRepository repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<ProviderDTO> Create(ProviderDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Provider was null.");
            }

            var provider = dto.ToDomain();

            if (repository.IsNotUnique(provider))
            {
                throw new ArgumentException(nameof(provider), "There is already an providerDto with such data");
            }

            var newOrganization = await repository.Create(provider).ConfigureAwait(false);

            return newOrganization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProviderDTO>> GetAll()
        {
             var providers = await repository.GetAll().ConfigureAwait(false);
            
             return providers.Select(organization => organization.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ProviderDTO> GetById(long id)
        {
            var provider = await repository.GetById(id).ConfigureAwait(false);
            if (provider == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return provider.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ProviderDTO> Update(ProviderDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Provider was null.");
            }

            if (dto.EDRPOU.Length == 0)
            {
                throw new ArgumentException("EDRPOU code is empty", nameof(dto));
            }

            if (dto.INPP.Length == 0)
            {
                throw new ArgumentException("INPP code is empty", nameof(dto));
            }

            if (dto.MFO.Length == 0)
            {
                throw new ArgumentException("MFO code is empty", nameof(dto));
            }

            if (dto.Title.Length == 0)
            {
                throw new ArgumentException("Title is empty", nameof(dto));
            }

            if (dto.Description.Length == 0)
            {
                throw new ArgumentException("Description is empty", nameof(dto));
            }

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
    }
}