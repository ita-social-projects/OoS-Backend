using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for InformationAboutPortal entity.
    /// </summary>
    public class AboutPortalService : IAboutPortalService
    {
        private const int LimitOfItems = 10;

        private readonly IAboutPortalRepository repository;
        private readonly ISensitiveEntityRepository<AboutPortal> informationAboutPortalRepository;
        private readonly ISensitiveEntityRepository<AboutPortalItem> informationAboutPortalItemRepository;
        private readonly ILogger<AboutPortalService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPortalService"/> class.
        /// </summary>
        /// <param name="informationAboutPortalRepository">InformationAboutPortal repository.</param>
        /// <param name="informationAboutPortalItemRepository">InformationAboutPortalItem repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public AboutPortalService(
            IAboutPortalRepository repository,
            ISensitiveEntityRepository<AboutPortal> informationAboutPortalRepository,
            ISensitiveEntityRepository<AboutPortalItem> informationAboutPortalItemRepository,
            ILogger<AboutPortalService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.repository = repository;
            this.informationAboutPortalRepository = informationAboutPortalRepository;
            this.informationAboutPortalItemRepository = informationAboutPortalItemRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> Create(AboutPortalDto informationAboutPortalDto)
        {
            logger.LogDebug("InformationAboutPortal creating was started.");

            if (informationAboutPortalDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalDto));
            }

            if (await informationAboutPortalRepository.Count().ConfigureAwait(false) > 0)
            {
                throw new InvalidOperationException("Cannot create more than one record about portal");
            }

            Func<Task<AboutPortal>> operation = async () =>
                await informationAboutPortalRepository.Create(mapper.Map<AboutPortal>(informationAboutPortalDto)).ConfigureAwait(false);

            var newInformationAboutPortal = await informationAboutPortalRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"InformationAboutPortal with Id = {newInformationAboutPortal?.Id} created successfully.");

            return mapper.Map<AboutPortalDto>(newInformationAboutPortal);
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> Update(AboutPortalDto informationAboutPortalDto)
        {
            logger.LogDebug("Updating InformationAboutPortal started.");

            AboutPortalDto updatedInformationAboutPortalDto;

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            if (!infoAboutPortals.Any())
            {
                updatedInformationAboutPortalDto = await Create(informationAboutPortalDto).ConfigureAwait(false);
            }
            else
            {
                var currentInformationAboutPortal = infoAboutPortals.Single();
                informationAboutPortalDto.Id = currentInformationAboutPortal.Id;
                await repository.DeleteAllItems().ConfigureAwait(false);
                mapper.Map(informationAboutPortalDto, currentInformationAboutPortal);
                var informationAboutPortal = await informationAboutPortalRepository.Update(currentInformationAboutPortal).ConfigureAwait(false);
                updatedInformationAboutPortalDto = mapper.Map<AboutPortalDto>(informationAboutPortal);
            }

            logger.LogDebug("Updating InformationAboutPortal finished.");

            return updatedInformationAboutPortalDto;
        }

        /// <inheritdoc/>
        public async Task<AboutPortalDto> GetInformationAboutPortal()
        {
            logger.LogDebug("Get information about portal is started.");

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            var informationAboutPortal = infoAboutPortals.SingleOrDefault();

            logger.LogDebug("Get information about portal is finished.");

            return mapper.Map<AboutPortalDto>(informationAboutPortal);
        }

        public async Task<AboutPortalItemDto> GetItemById(Guid id)
        {
            var informationAboutPortalItem = await informationAboutPortalItemRepository.GetById(id).ConfigureAwait(false);

            return mapper.Map<AboutPortalItemDto>(informationAboutPortalItem);
        }

        public async Task<AboutPortalItemDto> CreateItem(AboutPortalItemDto informationAboutPortalItemDto)
        {
            if (informationAboutPortalItemDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalItemDto));
            }

            if (await informationAboutPortalItemRepository.Count().ConfigureAwait(false) >= LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var informationAboutPortalItem = mapper.Map<AboutPortalItem>(informationAboutPortalItemDto);

            informationAboutPortalItem = await LinkPortalToItemAsync(informationAboutPortalItem).ConfigureAwait(false);

            Func<Task<AboutPortalItem>> operation = async () =>
                await informationAboutPortalItemRepository.Create(informationAboutPortalItem).ConfigureAwait(false);

            var newInformationAboutPortalItem = await informationAboutPortalItemRepository.RunInTransaction(operation).ConfigureAwait(false);

            return mapper.Map<AboutPortalItemDto>(newInformationAboutPortalItem);
        }

        public async Task<AboutPortalItemDto> UpdateItem(AboutPortalItemDto informationAboutPortalItemDto)
        {
            if (informationAboutPortalItemDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalItemDto));
            }

            var informationAboutPortalItem = mapper.Map<AboutPortalItem>(informationAboutPortalItemDto);

            informationAboutPortalItem = await LinkPortalToItemAsync(informationAboutPortalItem).ConfigureAwait(false);

            var informationAboutPortalItemUpdated = await informationAboutPortalItemRepository.Update(informationAboutPortalItem).ConfigureAwait(false);

            return mapper.Map<AboutPortalItemDto>(informationAboutPortalItemUpdated);
        }

        public async Task DeleteItem(Guid id)
        {
            try
            {
                var entity = await informationAboutPortalItemRepository.GetById(id).ConfigureAwait(false);

                await informationAboutPortalItemRepository.Delete(entity).ConfigureAwait(false);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AboutPortalItemDto>> GetAllItems()
        {
            var items = await informationAboutPortalItemRepository.GetAll().ConfigureAwait(false);

            return mapper.Map<List<AboutPortalItemDto>>(items);
        }

        private async Task<AboutPortalItem> LinkPortalToItemAsync(AboutPortalItem informationAboutPortalItem)
        {
            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            var informationAboutPortal = infoAboutPortals.Single();

            informationAboutPortalItem.AboutPortal = informationAboutPortal;
            return informationAboutPortalItem;
        }
    }
}
