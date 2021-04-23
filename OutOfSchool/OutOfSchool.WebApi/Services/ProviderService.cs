using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Implements the interface with CRUD functionality for Provider entity.
    /// </summary>
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository providerRepository;
        private readonly IEntityRepository<Address> addressRepository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="providerRepository">Provider repository.</param>
        /// <param name="addressRepository">Address repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ProviderService(IProviderRepository providerRepository, IEntityRepository<Address> addressRepository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.providerRepository = providerRepository;
            this.addressRepository = addressRepository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Create(ProviderDto dto)
        {
            logger.Information("Provider creating was started.");

            var provider = dto.ToDomain();

            if (providerRepository.Exists(provider))
            {
                throw new ArgumentException(localizer["There is already a provider with such a data."]);
            }

            var newProvider = await providerRepository.Create(provider).ConfigureAwait(false);

            return newProvider.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProviderDto>> GetAll()
        {
            logger.Information("Process of getting all Providers started.");

            var providers = await providerRepository.GetAll().ConfigureAwait(false);

            logger.Information(!providers.Any()
                ? "Provider table is empty."
                : "Successfully got all records from the Provider table.");

            return providers.Select(provider => provider.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetById(long id)
        {
            logger.Information("Process of getting Provider by id started.");

            var provider = await providerRepository.GetById(id).ConfigureAwait(false);

            if (provider == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Provider with id = {id}.");

            return provider.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Update(ProviderDto dto)
        {
            try
            {
                var provider = await providerRepository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Provider successfully updated.");

                return provider.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Provider in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Provider deleting was launched.");
        
            try
            {
                var entity = await providerRepository.GetById(id).ConfigureAwait(false);

                await providerRepository.Delete(entity).ConfigureAwait(false);

                if (entity.LegalAddressId == entity.ActualAddressId)
                {
                    await addressRepository.Delete(new Address { Id = entity.LegalAddressId }).ConfigureAwait(false);
                }
                else
                {
                    await addressRepository.Delete(new Address { Id = entity.LegalAddressId }).ConfigureAwait(false);
                    await addressRepository.Delete(new Address { Id = entity.ActualAddressId }).ConfigureAwait(false);
                }

                logger.Information("Provider successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Provider in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetByUserId(string id)
        {
            logger.Information("Process of getting Provider by User Id started.");

            Expression<Func<Provider, bool>> filter = p => p.UserId == id;

            var providers = await providerRepository.GetByFilter(filter).ConfigureAwait(false);

            if (!providers.Any())
            {
                throw new ArgumentException(localizer["There is no Provider in the Db with such User id"], nameof(id));                        
            }

            logger.Information($"Successfully got a Provider with User id = {id}.");

            return providers.FirstOrDefault().ToModel();
        }
    }
}