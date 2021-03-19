using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for ParentController.
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly IEntityRepository<Parent> repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        public ParentService(IEntityRepository<Parent> entityRepository)
        {
            this.repository = entityRepository;
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Create(ParentDTO parent)
        {
            Parent res = await repository.Create(parent.ToDomain()).ConfigureAwait(false);
            return res.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            Parent parent = await repository.GetById(id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException("This id doesn't exist", nameof(id));
            }

            await repository.Delete(parent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ParentDTO>> GetAll()
        {
            IEnumerable<Parent> parents = await this.repository.GetAll().ConfigureAwait(false);
            return parents.Select(x => x.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetById(long id)
        {
            Parent parent = await repository.GetById((int)id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException("Not Found", nameof(id));
            }

            return parent.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Update(ParentDTO parent)
        {
            if (parent == null)
            {
                throw new ArgumentException("Parent cannot be null", nameof(parent));
            }

            await UpdateValidation(parent).ConfigureAwait(false);
            Parent res = await repository.Update(parent.ToDomain()).ConfigureAwait(false);
            return res.ToModel();
        }

        private async Task UpdateValidation(ParentDTO parent)
        {
            Parent tmp = await repository.GetById((int)parent.Id).ConfigureAwait(false);
            if (tmp == null)
            {
                throw new ArgumentException("Wrong id", nameof(parent));
            }
        }
    }
}
