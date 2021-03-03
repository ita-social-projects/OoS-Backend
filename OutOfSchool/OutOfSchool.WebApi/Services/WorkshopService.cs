using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Workshop entity.
    /// </summary>
    public class WorkshopService : IWorkshopService
    {
        private readonly IEntityRepository<Workshop> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Workshop entity.</param>
        public WorkshopService(IEntityRepository<Workshop> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            var workshop = dto.ToDomain();

            var newWorkshop = await repository.Create(workshop).ConfigureAwait(false);

            return newWorkshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetAll()
        {
            var workshops = await repository.GetAll().ConfigureAwait(false);

            return workshops.Select(workshop => workshop.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await repository.GetById(id).ConfigureAwait(false);

            return workshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            var workshop = await repository.Update(dto.ToDomain()).ConfigureAwait(false);
            
            return workshop.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            var dtoToDelete = new Workshop() { Id = id };
                
            await repository.Delete(dtoToDelete).ConfigureAwait(false);
        }
    }
}