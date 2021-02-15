using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services.Implementation
{
    /// <summary>
    /// Service with business logic for Section model.
    /// </summary>
    public class SectionService : ISectionService
    {
        private IEntityRepository<Section> SectionRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionService"/> class.
        /// </summary>
        /// <param name="mapper">Mapper instance.</param>
        /// <param name="sectionRepository">Repository for Section entity.</param>
        public SectionService(IMapper mapper, IEntityRepository<Section> sectionRepository)
        {
            this.mapper = mapper;
            this.SectionRepository = sectionRepository;
        }

        /// <inheritdoc/>
        public async Task<SectionDTO> Create(SectionDTO section)
        {
            if (section == null)
            {
                throw new ArgumentNullException($"{nameof(SectionDTO)} entity must not be null");
            }

            try
            {
                var newSection = mapper.Map<SectionDTO, Section>(section);

                await SectionRepository.Create(newSection).ConfigureAwait(false);

                return section;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(SectionDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<SectionDTO> GetAllSections()
        {
            var sectionDto = SectionRepository.GetAll()
                .Select(section => mapper.Map<Section, SectionDTO>(section));

            return sectionDto;
        }
    }
}