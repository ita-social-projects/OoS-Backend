using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Workshop model.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private IEntityRepository<Workshop> WorkshopRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="mapper">Mapper instance.</param>
        /// <param name="workshopRepository">Repository for Workshop entity.</param>
        public WorkshopService(IMapper mapper, IEntityRepository<Workshop> workshopRepository)
        {
            this.mapper = mapper;
            WorkshopRepository = workshopRepository;
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
                var newWorkshop = mapper.Map<WorkshopDTO, Workshop>(workshop);

                await WorkshopRepository.Create(newWorkshop).ConfigureAwait(false);

                return workshop;
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(WorkshopDTO)} could not be saved: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetAllWorkshops()
        {
            var workshopDto = await Task.FromResult
            (
                WorkshopRepository.GetAll()
                    .Select(workshop => mapper.Map<Workshop, WorkshopDTO>(workshop))
            );

            return workshopDto;
        }
    }
}