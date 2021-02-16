using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Workshop model.
    /// </summary>
    public class SectionService : ISectionService
    {
        private IEntityRepository<Workshop> SectionRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionService"/> class.
        /// </summary>
        /// <param name="mapper">Mapper instance.</param>
        /// <param name="sectionRepository">Repository for Workshop entity.</param>
        public SectionService(IMapper mapper, IEntityRepository<Workshop> sectionRepository)
        {
            this.mapper = mapper;
            this.SectionRepository = sectionRepository;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO workshop)
        {
            if (workshop == null)
            {
                throw new ArgumentNullException($"{nameof(WorkshopDTO)} entity must not be null");
            }

            try
            {
                var newSection = mapper.Map<WorkshopDTO, Workshop>(workshop);

                await SectionRepository.Create(newSection).ConfigureAwait(false);

                return workshop;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(WorkshopDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<WorkshopDTO> GetAllSections()
        {
            var sectionDto = SectionRepository.GetAll()
                .Select(section => mapper.Map<Workshop, WorkshopDTO>(section));

            return sectionDto;
        }
    }
}