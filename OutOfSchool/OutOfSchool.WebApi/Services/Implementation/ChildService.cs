using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
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
        /// <param name="mapper">Mapper</param>
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
    }
}
