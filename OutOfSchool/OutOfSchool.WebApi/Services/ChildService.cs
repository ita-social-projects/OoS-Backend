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
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private readonly IEntityRepository<Child> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="repository">Repository for the Child entity.</param>
        public ChildService(IEntityRepository<Child> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO dto)
        {
            var child = dto.ToDomain();

            var newChild = await repository.Create(child).ConfigureAwait(false);

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChildDTO>> GetAll()
        {
            var children = await repository.GetAll().ConfigureAwait(false);

            return children.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            var child = await repository.GetById(id).ConfigureAwait(false);

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Update(ChildDTO dto)
        {
            var child = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            var dtoToDelete = new Child { Id = id };

            await repository.Delete(dtoToDelete).ConfigureAwait(false);
        }
    }
}