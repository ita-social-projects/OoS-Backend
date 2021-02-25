using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Organization entity.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private IOrganizationRepository repository;

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
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Organization was null.");
            }

            var organization = dto.ToDomain();

            if (!repository.IsUnique(organization))
            {
                throw new ArgumentException(nameof(organization), "There is already an organizationDto with such data");
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
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return organization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Update(OrganizationDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "Organization was null.");
            }

            if (dto.EDRPOU.Length == 0)
            {
                throw new ArgumentException("EDRPOU code is empty", nameof(dto));
            }

            if (dto.INPP.Length == 0)
            {
                throw new ArgumentException("INPP code is empty", nameof(dto));
            }

            if (dto.MFO.Length == 0)
            {
                throw new ArgumentException("MFO code is empty", nameof(dto));
            }

            if (dto.Title.Length == 0)
            {
                throw new ArgumentException("Title is empty", nameof(dto));
            }

            if (dto.Description.Length == 0)
            {
                throw new ArgumentException("Description is empty", nameof(dto));
            }

            try
            {
                var organization = await repository.Update(dto.ToDomain()).ConfigureAwait(false);
              
                return organization.ToModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(dto)} could not be updated: {ex.Message}");
            }
        }
        
        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            try
            {
                await repository
                    .Delete(await repository.GetById(id).ConfigureAwait(false))
                    .ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }
        }
    }
}