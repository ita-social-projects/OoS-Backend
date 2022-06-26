using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Provider entity.
    /// </summary>
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository providerRepository;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly IRatingService ratingService;
        private readonly ILogger<ProviderService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;
        private readonly IEntityRepository<Address> addressRepository;
        private readonly IWorkshopServicesCombiner workshopServiceCombiner;
        private readonly IChangesLogService changesLogService;

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
        /// <param name="mapper">Mapper.</param>
        /// <param name="addressRepository">AddressRepository.</param>
        /// <param name="workshopServiceCombiner">WorkshopServiceCombiner.</param>
        /// <param name="providerAdminRepository">Provider admin repository.</param>
        /// <param name="providerImagesService">Images service.</param>
        /// <param name="changesLogService">ChangesLogService.</param>
        public ProviderService(
            IProviderRepository providerRepository,
            IEntityRepository<User> usersRepository,
            IRatingService ratingService,
            ILogger<ProviderService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IEntityRepository<Address> addressRepository,
            IWorkshopServicesCombiner workshopServiceCombiner,
            IProviderAdminRepository providerAdminRepository,
            IImageDependentEntityImagesInteractionService<Provider> providerImagesService,
            IChangesLogService changesLogService)
        {
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            this.ratingService = ratingService ?? throw new ArgumentNullException(nameof(ratingService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.workshopServiceCombiner = workshopServiceCombiner ?? throw new ArgumentNullException(nameof(workshopServiceCombiner));
            this.providerAdminRepository = providerAdminRepository ?? throw new ArgumentNullException(nameof(providerAdminRepository));
            ProviderImagesService = providerImagesService ?? throw new ArgumentNullException(nameof(providerImagesService));
            this.changesLogService = changesLogService;
        }

        private protected IImageDependentEntityImagesInteractionService<Provider> ProviderImagesService { get; }

        /// <inheritdoc/>
        public async Task<ProviderDto> Create(ProviderDto providerDto)
            => await CreateProviderWithActionAfterAsync(providerDto).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<IEnumerable<ProviderDto>> GetAll()
        {
            logger.LogDebug("Getting all Providers started.");

            var providers = await providerRepository.GetAll().ConfigureAwait(false);

            logger.LogInformation(!providers.Any()
                ? "Provider table is empty."
                : $"All {providers.Count()} records were successfully received from the Provider table");

            var providersDTO = providers.Select(provider => mapper.Map<ProviderDto>(provider)).ToList();

            // TODO: move ratings calculations out of getting all providers.
            FillRatingsForProviders(providersDTO);

            return providersDTO;
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ProviderDto>> GetByFilter(SearchStringFilter filter)
        {
            logger.LogInformation("Getting all Providers started (by filter).");

            if (filter is null)
            {
                filter = new SearchStringFilter();
            }

            var filterPredicate = PredicateBuild(filter);

            int count = await providerRepository.Count(filterPredicate).ConfigureAwait(false);
            var providers = await providerRepository
                .Get<string>(filter.From, filter.Size, string.Empty, filterPredicate, x => x.User.FirstName, true)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogInformation(!providers.Any()
                ? "Parents table is empty."
                : $"All {providers.Count} records were successfully received from the Parent table");

            var providersDTO = providers.Select(provider => mapper.Map<ProviderDto>(provider)).ToList();
            FillRatingsForProviders(providersDTO);

            var result = new SearchResult<ProviderDto>()
            {
                TotalAmount = count,
                Entities = providersDTO,
            };

            return result;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetById(Guid id)
        {
            logger.LogInformation($"Getting Provider by Id started. Looking Id = {id}.");
            var provider = await providerRepository.GetById(id).ConfigureAwait(false);

            if (provider == null)
            {
                return null;
            }

            logger.LogInformation($"Successfully got a Provider with Id = {id}.");

            var providerDTO = mapper.Map<ProviderDto>(provider);

            var rating = ratingService.GetAverageRating(providerDTO.Id, RatingType.Provider);

            providerDTO.Rating = rating?.Item1 ?? default;
            providerDTO.NumberOfRatings = rating?.Item2 ?? default;

            return providerDTO;
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> GetByUserId(string id, bool isDeputyOrAdmin = false)
        {
            logger.LogInformation($"Getting Provider by UserId started. Looking UserId is {id}.");
            Provider provider = default;

            if (isDeputyOrAdmin)
            {
                var providerAdmins = await providerAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false);
                var providerAdmin = providerAdmins.FirstOrDefault();
                if (providerAdmin != null)
                {
                    provider = providerAdmin.Provider;
                }
            }
            else
            {
                var providers = await providerRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false);
                provider = providers.FirstOrDefault();
            }

            if (provider == null)
            {
                throw new ArgumentException(localizer["There is no Provider in the Db with such User id"], nameof(id));
            }

            logger.LogInformation($"Successfully got a Provider with UserId = {id}.");

            return mapper.Map<ProviderDto>(provider);
        }

        /// <inheritdoc/>
        public async Task<ProviderDto> Update(ProviderDto providerDto, string userId)
            => await UpdateProviderWithActionBeforeSavingChanges(providerDto, userId).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task Delete(Guid id) => await DeleteProviderWithActionBefore(id).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<Guid> GetProviderIdForWorkshopById(Guid workshopId) =>
            await workshopServiceCombiner.GetWorkshopProviderId(workshopId).ConfigureAwait(false);

        private protected async Task<ProviderDto> CreateProviderWithActionAfterAsync(ProviderDto providerDto, Func<Provider, Task> actionAfterCreation = null)
        {
            _ = providerDto ?? throw new ArgumentNullException(nameof(providerDto));

            logger.LogDebug("Provider creating was started");

            if (providerRepository.ExistsUserId(providerDto.UserId))
            {
                throw new InvalidOperationException(localizer["You can not create more than one account."]);
            }

            // Note: clear the actual address if it is equal to the legal to avoid data duplication in the database
            if (providerDto.LegalAddress.Equals(providerDto.ActualAddress))
            {
                providerDto.ActualAddress = null;
            }

            var providerDomainModel = mapper.Map<Provider>(providerDto);

            // BUG: concurrency issue:
            //      while first repository with this particular user id is not saved to DB - we can create any number of repositories for this user.
            if (providerRepository.SameExists(providerDomainModel))
            {
                throw new InvalidOperationException(localizer["There is already a provider with such a data"]);
            }

            var users = await usersRepository.GetByFilter(u => u.Id.Equals(providerDto.UserId)).ConfigureAwait(false);
            providerDomainModel.User = users.Single();
            providerDomainModel.User.IsRegistered = true;

            var newProvider = await providerRepository.Create(providerDomainModel).ConfigureAwait(false);

            if (actionAfterCreation != null)
            {
                await actionAfterCreation(newProvider).ConfigureAwait(false);
                await UpdateProvider().ConfigureAwait(false);
            }

            logger.LogDebug("Provider with Id = {ProviderId} created successfully", newProvider?.Id);

            return mapper.Map<ProviderDto>(newProvider);
        }

        private protected async Task<ProviderDto> UpdateProviderWithActionBeforeSavingChanges(ProviderDto providerDto, string userId, Func<Provider, Task> actionBeforeUpdating = null)
        {
            _ = providerDto ?? throw new ArgumentNullException(nameof(providerDto));
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            logger.LogDebug("Updating Provider with Id = {Id} was started", providerDto.Id);

            try
            {
                var checkProvider =
                    (await providerRepository.GetByFilter(p => p.Id == providerDto.Id).ConfigureAwait(false))
                    .FirstOrDefault();

                if (checkProvider?.UserId != userId)
                {
                    return null;
                }

                providerDto.LegalAddress.Id = checkProvider.LegalAddress.Id;

                if (providerDto.LegalAddress.Equals(providerDto.ActualAddress))
                {
                    providerDto.ActualAddress = null;
                }

                if (providerDto.ActualAddress is null && checkProvider.ActualAddress is { })
                {
                    var checkProviderActualAddress = checkProvider.ActualAddress;
                    checkProvider.ActualAddressId = null;
                    checkProvider.ActualAddress = null;
                    mapper.Map(providerDto, checkProvider);
                    await addressRepository.Delete(checkProviderActualAddress).ConfigureAwait(false);
                }
                else
                {
                    if (providerDto.ActualAddress != null)
                    {
                        providerDto.ActualAddress.Id = checkProvider.ActualAddress?.Id ?? 0;
                    }

                    if (IsNeedInRelatedWorkshopsUpdating(providerDto, checkProvider))
                    {
                        checkProvider = await providerRepository.RunInTransaction(async () =>
                        {
                            var workshops = await workshopServiceCombiner
                                .PartialUpdateByProvider(mapper.Map<Provider>(providerDto))
                                .ConfigureAwait(false);

                            mapper.Map(providerDto, checkProvider);
                            LogProviderChanges(checkProvider, userId);
                            await UpdateProvider().ConfigureAwait(false);

                            foreach (var workshop in workshops)
                            {
                                logger.LogInformation($"Provider's properties with Id = {checkProvider?.Id} " +
                                                      $"in workshops with Id = {workshop?.Id} updated successfully.");
                            }

                            return checkProvider;
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        mapper.Map(providerDto, checkProvider);
                    }

                    if (actionBeforeUpdating != null)
                    {
                        await actionBeforeUpdating(checkProvider).ConfigureAwait(false);
                    }

                    LogProviderChanges(checkProvider, userId);
                    await UpdateProvider().ConfigureAwait(false);
                }

                logger.LogInformation("Provider with Id = {CheckProviderId} was updated successfully", checkProvider?.Id);

                return mapper.Map<ProviderDto>(checkProvider);
            }
            finally
            {
                logger.LogTrace("Updating Provider with Id = {Id} was finished", providerDto.Id);
            }
        }

        private protected async Task DeleteProviderWithActionBefore(Guid id, Func<Provider, Task> actionBeforeDeleting = null)
        {
            // BUG: Possible bug with deleting provider not owned by the user itself.
            // TODO: add unit tests to check ownership functionality
            logger.LogInformation("Deleting Provider with Id = {Id} started", id);

            try
            {
                var entity = await providerRepository.GetById(id).ConfigureAwait(false);

                if (actionBeforeDeleting != null)
                {
                    await actionBeforeDeleting(entity).ConfigureAwait(false);
                }

                await providerRepository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation("Provider with Id = {Id} successfully deleted", id);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Deleting failed. Provider with Id = {Id} doesn't exist in the system", id);
                throw;
            }
        }

        private static bool IsNeedInRelatedWorkshopsUpdating(ProviderDto providerDto, Provider checkProvider)
        {
            return checkProvider.FullTitle != providerDto.FullTitle
                   || checkProvider.Ownership != providerDto.Ownership;
        }

        private async Task UpdateProvider()
        {
            try
            {
                await providerRepository.UnitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Updating a provider failed");
                throw;
            }
        }

        private void LogProviderChanges(Provider provider, string userId)
        {
            changesLogService.AddEntityChangesToDbContext(provider, userId);
        }

        private Expression<Func<Provider, bool>> PredicateBuild(SearchStringFilter filter)
        {
            var predicate = PredicateBuilder.True<Provider>();

            if (!string.IsNullOrWhiteSpace(filter.SearchString))
            {
                var tempPredicate = PredicateBuilder.False<Provider>();

                foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.User.FirstName.StartsWith(word, StringComparison.InvariantCulture)
                            || x.User.LastName.StartsWith(word, StringComparison.InvariantCulture)
                            || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCulture)
                            || x.Email.StartsWith(word, StringComparison.InvariantCulture)
                            || x.PhoneNumber.Contains(word, StringComparison.InvariantCulture));
                }

                predicate = predicate.And(tempPredicate);
            }

            return predicate;
        }

        private void FillRatingsForProviders(List<ProviderDto> providersDTO)
        {
            var averageRatings =
                ratingService.GetAverageRatingForRange(providersDTO.Select(p => p.Id), RatingType.Provider);

            foreach (var provider in providersDTO)
            {
                var averageRatingsForProvider = averageRatings.FirstOrDefault(r => r.Key == provider.Id);
                if (averageRatingsForProvider.Key != Guid.Empty)
                {
                    var (_, (rating, numberOfVotes)) = averageRatingsForProvider;
                    provider.Rating = rating;
                    provider.NumberOfRatings = numberOfVotes;
                }
            }
        }
    }
}