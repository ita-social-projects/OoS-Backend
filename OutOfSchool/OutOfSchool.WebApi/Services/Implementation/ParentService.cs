using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Implementation
{
    /// <summary>
    /// Service with business logic for ParentController
    /// </summary>
    public class ParentService : IParentService
    {
        private IEntityRepository<Parent> ParentRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="mapper">Mapper.</param>
        public ParentService(IEntityRepository<Parent> entityRepository, IMapper mapper)
        {
            this.ParentRepository = entityRepository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> Create(ParentDTO parent)
        {
            if (parent == null)
            {
                throw new ArgumentException(nameof(parent), "Parent is null");
            }

            if (parent.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(parent));
            }

            if (parent.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(parent));
            }

            if (parent.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(parent));
            }

            Parent res = await this.ParentRepository.Create(this.mapper.Map<ParentDTO, Parent>(parent)).ConfigureAwait(false);
            return this.mapper.Map<Parent, ParentDTO>(res);
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            Parent parent = await this.ParentRepository.GetById(id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException(nameof(id), "This id doesn't exist");
            }

            await this.ParentRepository.Delete(parent).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEnumerable<ParentDTO> GetAll()
        {
            IEnumerable<Parent> parents = this.ParentRepository.GetAll().ToList();
            return this.mapper.Map<IEnumerable<Parent>, IEnumerable<ParentDTO>>(parents);
        }

        /// <inheritdoc/>
        public async Task<ParentDTO> GetById(long id)
        {
            Parent parent = await this.ParentRepository.GetById((int)id).ConfigureAwait(false);
            if (parent == null)
            {
                throw new ArgumentException(nameof(id), "Not Found");
            }

            return this.mapper.Map<Parent, ParentDTO>(parent);
        }

        /// <inheritdoc/>
        public async Task Update(ParentDTO parent)
        {
            if (parent == null)
            {
                throw new ArgumentException(nameof(parent), "Parent is null");
            }

            if (parent.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(parent));
            }

            if (parent.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(parent));
            }

            if (parent.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(parent));
            }

            Parent tmp = await this.ParentRepository.GetById((int)parent.Id).ConfigureAwait(false);
            if (tmp == null)
            {
                throw new ArgumentException(nameof(parent), "Wrong id");
            }

            this.ParentRepository.Update(this.mapper.Map<ParentDTO, Parent>(parent));
        }
    }
}
