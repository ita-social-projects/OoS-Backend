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
    ///  Service with business logic for Organization model.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private IOrganizationRepository OrganizationRepository { get; set; }
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="entityRepository">Repository for some entity.</param>
        /// <param name="mapper">Mapper.</param>
        public OrganizationService(IOrganizationRepository entityRepository, IMapper mapper)
        {
            this.OrganizationRepository = entityRepository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Create(OrganizationDTO organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Organization was null.");
            }

            Organization newOrganization = this.mapper.Map<OrganizationDTO, Organization>(organization);
          

            if(OrganizationRepository.IsUnique(newOrganization))
            {              
                throw new ArgumentException(nameof(newOrganization), "There is already an organization with such data");
            }
            else
            {
                var organization_ = await this.OrganizationRepository.Create(newOrganization).ConfigureAwait(false);
                return await Task.FromResult(this.mapper.Map<Organization, OrganizationDTO>(organization_)).ConfigureAwait(false);
            }         
        }

        /// <inheritdoc/>
        public IEnumerable<OrganizationDTO> GetAll()
        {
            return this.OrganizationRepository.GetAll().Select(
                x => this.mapper.Map<Organization, OrganizationDTO>(x));
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> GetById(long id)
        {
            var organization = this.OrganizationRepository.GetById(id).Result;
            if (organization == null)
            {
                throw new ArgumentException("Incorrect Id!", nameof(id));
            }

            return await Task.Run(() =>
            {
                return this.mapper.Map<Organization, OrganizationDTO>(organization);
            }).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Update(OrganizationDTO organizationDTO)
        {
            if (organizationDTO == null)
            {
                throw new ArgumentNullException(nameof(organizationDTO), "Organization was null.");
            }

            if (organizationDTO.EDRPOU.Length == 0)
            {
                throw new ArgumentException("EDRPOU code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.INPP.Length == 0)
            {
                throw new ArgumentException("INPP code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.MFO.Length == 0)
            {
                throw new ArgumentException("MFO code is empty", nameof(organizationDTO));
            }

            if (organizationDTO.Title.Length == 0)
            {
                throw new ArgumentException("Title is empty", nameof(organizationDTO));
            }

            if (organizationDTO.Description.Length == 0)
            {
                throw new ArgumentException("Description is empty", nameof(organizationDTO));
            }

            Organization organization = this.mapper.Map<OrganizationDTO, Organization>(organizationDTO);
            this.OrganizationRepository.Update(organization);
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            OrganizationDTO organizationDTO;
            try
            {
                organizationDTO = await this.GetById(id).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(nameof(id), ex.Message);
            }

            Organization organization = this.mapper.Map<OrganizationDTO, Organization>(organizationDTO);
            await this.OrganizationRepository.Delete(organization).ConfigureAwait(false);
        }
    }
}
