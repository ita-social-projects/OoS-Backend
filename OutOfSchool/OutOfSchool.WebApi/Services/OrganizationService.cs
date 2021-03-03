using System;
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
    /// Implements the interface with CRUD functionality for Organization entity.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="repository">Repository for some entity.</param>
        public OrganizationService(IOrganizationRepository repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Create(OrganizationDTO dto)
        {
            var organization = dto.ToDomain();

            if (!repository.IsUnique(organization))
            {
                throw new ArgumentException(nameof(organization), "There is already an organization with a such data");
            }

            var newOrganization = await repository.Create(organization).ConfigureAwait(false);

            return newOrganization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrganizationDTO>> GetAll()
        {
            var organizations = await repository.GetAll().ConfigureAwait(false);

            return organizations.Select(organization => organization.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> GetById(long id)
        {
            var organization = await repository.GetById(id).ConfigureAwait(false);
            if (organization == null)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id is greater than number of table entities.");
            }

            return organization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Update(OrganizationDTO dto)
        {
            var organization = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

            return organization.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            var dtoToDelete = new Organization() { Id = id };

            await repository.Delete(dtoToDelete).ConfigureAwait(false);
        }
    }
}