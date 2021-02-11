using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Service with business logic for Child model.
    /// </summary>
    public class ChildService : IChildService
    {
        private IEntityRepository<Child> ChildRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="mapper">Mapper.</param>
        public ChildService(IEntityRepository<Child> entityRepository, IMapper mapper)
        {
            this.ChildRepository = entityRepository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> Create(ChildDTO child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child), "Child was null.");
            }

            if (child.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Invalid Date of birth");
            }

            Child newChild = this.mapper.Map<ChildDTO, Child>(child);
            var child_ = await this.ChildRepository.Create(newChild).ConfigureAwait(false);
            return await Task.FromResult(this.mapper.Map<Child, ChildDTO>(child_)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEnumerable<ChildDTO> GetAll()
        {
            IEnumerable<ChildDTO> childrenDTO = this.ChildRepository.GetAll().Select(
                x => this.mapper.Map<Child, ChildDTO>(x));

            return childrenDTO;
        }

        /// <inheritdoc/>
        public async Task<ChildDTO> GetById(long id)
        {
            Child child = this.ChildRepository.GetById(id).Result;
            if (child == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return await Task.Run(() =>
            {
                return this.mapper.Map<Child, ChildDTO>(child);
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Update(ChildDTO childDTO)
        {
            if (childDTO == null)
            {
                throw new ArgumentNullException(nameof(childDTO), "Child was null.");
            }

            if (childDTO.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException("Wrong date of birth.", nameof(childDTO));
            }

            if (childDTO.FirstName.Length == 0)
            {
                throw new ArgumentException("Empty firstname.", nameof(childDTO));
            }

            if (childDTO.LastName.Length == 0)
            {
                throw new ArgumentException("Empty lastname.", nameof(childDTO));
            }

            if (childDTO.MiddleName.Length == 0)
            {
                throw new ArgumentException("Empty middlename.", nameof(childDTO));
            }

            Child child = this.mapper.Map<ChildDTO, Child>(childDTO);
            this.ChildRepository.Update(child);
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            ChildDTO childDTO;
            try
            {
                childDTO = await this.GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            Child child = this.mapper.Map<ChildDTO, Child>(childDTO);
            await this.ChildRepository.Delete(child).ConfigureAwait(false);
        }
    }
}
