using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
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
        private readonly IRatingService ratingService;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="providerRepository">Provider repository.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ProviderService(
            IProviderRepository providerRepository,
            IRatingService ratingService,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.providerRepository = providerRepository;
            this.ratingService = ratingService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Create(ProviderDto dto)
        {
            logger.Information("Provider creating was started.");

            if (providerRepository.Exists(dto.ToDomain()))
            {
                throw new ArgumentException(localizer["There is already a provider with such a data."]);
            }

            if (providerRepository.ExistsUserId(dto.UserId))
            {
                throw new ArgumentException(localizer["You can not create more than one account."]);
            }

            Func<Task<Provider>> operation = async () => await providerRepository.Create(dto.ToDomain()).ConfigureAwait(false);

            var newProvider = await providerRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.Information($"Provider with Id = {newProvider?.Id} created successfully.");

            return newProvider.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProviderDto>> GetAll()
        {
            logger.Information("Getting all Providers started.");

            var providers = await providerRepository.GetAll().ConfigureAwait(false);

            logger.Information(!providers.Any()
                ? "Provider table is empty."
                : $"All {providers.Count()} records were successfully received from the Provider table");

            var providersDTO = providers.Select(provider => provider.ToModel()).ToList();

            var averageRatings = ratingService.GetAverageRatingForRange(providersDTO.Select(p => p.Id), RatingType.Provider);

            if (averageRatings != null)
            {
                foreach (var provider in providersDTO)
                {
                    provider.Rating = averageRatings.FirstOrDefault(r => r.Key == provider.Id).Value;
                }
            }

            return providersDTO;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetById(long id)
        {
            logger.Information($"Getting Provider by Id started. Looking Id = {id}.");

            var provider = await providerRepository.GetById(id).ConfigureAwait(false);

            if (provider == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got a Provider with Id = {id}.");

            var providerDTO = provider.ToModel();

            providerDTO.Rating = ratingService.GetAverageRating(providerDTO.Id, RatingType.Provider);

            return providerDTO;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetByUserId(string id)
        {
            logger.Information($"Getting Provider by UserId started. Looking UserId is {id}.");

            Expression<Func<Provider, bool>> filter = p => p.UserId == id;

            var providers = await providerRepository.GetByFilter(filter).ConfigureAwait(false);

            if (!providers.Any())
            {
                throw new ArgumentException(localizer["There is no Provider in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got a Provider with UserId = {id}.");

            return providers.FirstOrDefault().ToModel();
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Update(ProviderDto dto)
        {
            logger.Information($"Updating Provider with Id = {dto?.Id} started.");

            try
            {
                var provider = await providerRepository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information($"Provider with Id = {provider?.Id} updated succesfully.");

                return provider.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Provider with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information($"Deleting Provider with Id = {id} started.");

            try
            {
                var entity = await providerRepository.GetById(id).ConfigureAwait(false);

                await providerRepository.Delete(entity).ConfigureAwait(false);

                logger.Information($"Provider with Id = {id} succesfully deleted.");
            }
            catch (ArgumentNullException)
            {
                logger.Error($"Deleting failed. Provider with Id = {id} doesn't exist in the system.");
                throw;
            }
        }
    }
}