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
        private const string AdminRole = "admin";
        private readonly IProviderRepository providerRepository;
        private readonly IRatingService ratingService;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        // TODO: It should be removed after models revision.
        //       Temporary instance to fill 'Provider' model 'User' property
        private readonly IEntityRepository<User> usersRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderService"/> class.
        /// </summary>
        /// <param name="providerRepository">Provider repository.</param>
        /// <param name="usersRepository"><see cref="IEntityRepository{User}"/> repository object.</param>
        /// <param name="ratingService">Rating service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ProviderService(
            IProviderRepository providerRepository,
            IEntityRepository<User> usersRepository,
            IRatingService ratingService,
            ILogger logger,
            IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            this.ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Create(ProviderDto providerDto)
        {
            logger.Debug("Provider creating was started.");

            if (providerDto == null)
            {
                throw new ArgumentNullException(nameof(providerDto));
            }

            if (providerRepository.ExistsUserId(providerDto.UserId))
            {
                throw new InvalidOperationException(localizer["You can not create more than one account."]);
            }

            // TODO: Q: clear actual address if it is equal to the legal ?
            if (providerDto.ActualAddress == null
                || providerDto.ActualAddress.Equals(providerDto.LegalAddress))
            {
                providerDto.ActualAddress = providerDto.LegalAddress;
            }

            var providerDomainModel = providerDto.ToDomain();
            if (providerRepository.SameExists(providerDomainModel))
            {
                throw new InvalidOperationException(localizer["There is already a provider with such a data."]);
            }

            var users = await usersRepository.GetByFilter(u => string.Equals(u.Id, providerDto.UserId, StringComparison.Ordinal)).ConfigureAwait(false);
            providerDomainModel.User = users.SingleOrDefault();
            providerDomainModel.User.IsRegistered = true;

            var newProvider = await providerRepository.Create(providerDomainModel).ConfigureAwait(false);

            logger.Debug($"Provider with Id = {newProvider?.Id} created successfully.");

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
                    var ratingTuple = averageRatings.FirstOrDefault(r => r.Key == provider.Id);
                    provider.Rating = ratingTuple.Value?.Item1 ?? default;
                    provider.NumberOfRatings = ratingTuple.Value?.Item2 ?? default;
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

            var rating = ratingService.GetAverageRating(providerDTO.Id, RatingType.Provider);

            providerDTO.Rating = rating?.Item1 ?? default;
            providerDTO.NumberOfRatings = rating?.Item2 ?? default;

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
        public async Task<ProviderDto> Update(ProviderDto providerDto, string userId, string userRole)
        {
            logger.Debug($"Updating Provider with Id = {providerDto?.Id} started.");

            try
            {
                var checkProvider = providerRepository.GetByFilterNoTracking(p => p.Id == providerDto.Id).FirstOrDefault();

                if (checkProvider?.UserId == userId || userRole == AdminRole)
                {
                    var provider = await providerRepository.Update(providerDto.ToDomain()).ConfigureAwait(false);

                    logger.Information($"Provider with Id = {provider?.Id} updated succesfully.");

                    return provider.ToModel();
                }
                else
                {
                    return null;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error($"Updating failed. Provider with Id = {providerDto?.Id} doesn't exist in the system.");
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