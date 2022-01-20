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
    public class InformationAboutPortalService : IInformationAboutPortalService
    {
        private const int LimitOfItems = 10;

        private readonly ISensitiveEntityRepository<InformationAboutPortal> informationAboutPortalRepository;
        private readonly ISensitiveEntityRepository<InformationAboutPortalItem> informationAboutPortalItemRepository;
        private readonly ILogger<InformationAboutPortalService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationAboutPortalService"/> class.
        /// </summary>
        /// <param name="informationAboutPortalRepository">InformationAboutPortal repository.</param>
        /// <param name="informationAboutPortalItemRepository">InformationAboutPortalItem repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public InformationAboutPortalService(
            ISensitiveEntityRepository<InformationAboutPortal> informationAboutPortalRepository,
            ISensitiveEntityRepository<InformationAboutPortalItem> informationAboutPortalItemRepository,
            ILogger<InformationAboutPortalService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper)
        {
            this.informationAboutPortalRepository = informationAboutPortalRepository;
            this.informationAboutPortalItemRepository = informationAboutPortalItemRepository;
            this.logger = logger;
            this.localizer = localizer;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> Create(InformationAboutPortalDto informationAboutPortalDto)
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

            Func<Task<InformationAboutPortal>> operation = async () =>
                await informationAboutPortalRepository.Create(mapper.Map<InformationAboutPortal>(informationAboutPortalDto)).ConfigureAwait(false);

            var newInformationAboutPortal = await informationAboutPortalRepository.RunInTransaction(operation).ConfigureAwait(false);

            logger.LogDebug($"InformationAboutPortal with Id = {newInformationAboutPortal?.Id} created successfully.");

            return mapper.Map<InformationAboutPortalDto>(newInformationAboutPortal);
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> Update(InformationAboutPortalDto informationAboutPortalDto)
        {
            logger.LogDebug("Updating InformationAboutPortal started.");

            InformationAboutPortalDto updatedInformationAboutPortalDto;

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            if (!infoAboutPortals.Any())
            {
                updatedInformationAboutPortalDto = await Create(informationAboutPortalDto).ConfigureAwait(false);
            }
            else
            {
                InformationAboutPortal currentInformationAboutPortal = infoAboutPortals.Single();
                mapper.Map(informationAboutPortalDto, currentInformationAboutPortal);
                var informationAboutPortal = await informationAboutPortalRepository.Update(currentInformationAboutPortal).ConfigureAwait(false);
                updatedInformationAboutPortalDto = mapper.Map<InformationAboutPortalDto>(informationAboutPortal);
            }

            logger.LogDebug("Updating InformationAboutPortal finished.");

            return updatedInformationAboutPortalDto;
        }

        /// <inheritdoc/>
        public async Task<InformationAboutPortalDto> GetInformationAboutPortal()
        {
            logger.LogDebug("Get information about portal is started.");

            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            var informationAboutPortal = infoAboutPortals.SingleOrDefault();

            logger.LogDebug("Get information about portal is finished.");

            return mapper.Map<InformationAboutPortalDto>(informationAboutPortal);
        }

        public async Task<InformationAboutPortalItemDto> GetItemById(Guid id)
        {
            var informationAboutPortalItem = await informationAboutPortalItemRepository.GetById(id).ConfigureAwait(false);

            return mapper.Map<InformationAboutPortalItemDto>(informationAboutPortalItem);
        }

        public async Task<InformationAboutPortalItemDto> CreateItem(InformationAboutPortalItemDto informationAboutPortalItemDto)
        {
            if (informationAboutPortalItemDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalItemDto));
            }

            if (await informationAboutPortalItemRepository.Count().ConfigureAwait(false) >= LimitOfItems)
            {
                throw new InvalidOperationException($"Cannot create more than {LimitOfItems} items.");
            }

            var informationAboutPortalItem = mapper.Map<InformationAboutPortalItem>(informationAboutPortalItemDto);

            informationAboutPortalItem = await LinkPortalToItemAsync(informationAboutPortalItem).ConfigureAwait(false);

            Func<Task<InformationAboutPortalItem>> operation = async () =>
                await informationAboutPortalItemRepository.Create(informationAboutPortalItem).ConfigureAwait(false);

            var newInformationAboutPortalItem = await informationAboutPortalItemRepository.RunInTransaction(operation).ConfigureAwait(false);

            return mapper.Map<InformationAboutPortalItemDto>(newInformationAboutPortalItem);
        }

        public async Task<InformationAboutPortalItemDto> UpdateItem(InformationAboutPortalItemDto informationAboutPortalItemDto)
        {
            if (informationAboutPortalItemDto == null)
            {
                throw new ArgumentNullException(nameof(informationAboutPortalItemDto));
            }

            var informationAboutPortalItem = mapper.Map<InformationAboutPortalItem>(informationAboutPortalItemDto);

            informationAboutPortalItem = await LinkPortalToItemAsync(informationAboutPortalItem).ConfigureAwait(false);

            var informationAboutPortalItemUpdated = await informationAboutPortalItemRepository.Update(informationAboutPortalItem).ConfigureAwait(false);

            return mapper.Map<InformationAboutPortalItemDto>(informationAboutPortalItemUpdated);
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

        public async Task<IEnumerable<InformationAboutPortalItemDto>> GetAllItems()
        {
            var items = await informationAboutPortalItemRepository.GetAll().ConfigureAwait(false);

            return mapper.Map<List<InformationAboutPortalItemDto>>(items);
        }

        private async Task<InformationAboutPortalItem> LinkPortalToItemAsync(InformationAboutPortalItem informationAboutPortalItem)
        {
            var infoAboutPortals = await informationAboutPortalRepository.GetAll().ConfigureAwait(false);
            var informationAboutPortal = infoAboutPortals.Single();

            informationAboutPortalItem.InformationAboutPortal = informationAboutPortal;
            return informationAboutPortalItem;
        }
    }
}
