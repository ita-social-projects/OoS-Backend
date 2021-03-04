using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Organization entity.
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository repository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationService"/> class.
        /// </summary>
        /// <param name="repository">Repository for some entity.</param>
        public OrganizationService(IOrganizationRepository repository, ILogger logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Create(OrganizationDTO dto)
        {
            logger.Information("Organization creating was started.");
            
            var organization = dto.ToDomain();
            
            if (!repository.IsUnique(organization))
            {
                throw new ArgumentException(nameof(organization), "There is already an organization with a such data");
            }
            
            try
            {
                var newOrganization = await repository.Create(organization).ConfigureAwait(false);

                return newOrganization.ToModel();
            }
            catch (DbUpdateException)
            {
                logger.Error("Creating failed. Verify all information you have entered are valid.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<OrganizationDTO>> GetAll()
        {
            logger.Information("Process of getting all Organizations started.");
            
            var organizations = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!organizations.Any()
                ? "Organization table is empty."
                : "Successfully got all records from the Organization table.");
            
            return organizations.Select(organization => organization.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> GetById(long id)
        {
            logger.Information("Process of getting Organization by id started.");
            
            var organization = await repository.GetById(id).ConfigureAwait(false);
           
            if (organization == null)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id cannot be greater number of table entities.");
            }

            logger.Information($"Successfully got a Organization with id = {id}.");
            
            return organization.ToModel();
        }

        /// <inheritdoc/>
        public async Task<OrganizationDTO> Update(OrganizationDTO dto)
        {
            try
            {
                var organization = await repository.Update(dto.ToDomain()).ConfigureAwait(false);

                logger.Information("Updating successfully finished.");

                return organization.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed. There is no Organization in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            var dtoToDelete = new Organization() {Id = id};

            try
            {
                await repository.Delete(dtoToDelete).ConfigureAwait(false);

                logger.Information("Deleting successfully finished.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Organization in the Db with such an id.");
                throw;
            }
        }
    }
}