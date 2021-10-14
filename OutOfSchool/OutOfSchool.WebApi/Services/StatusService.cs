using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for for InstitutionStatus entity.
    /// </summary>
    public class StatusService : IStatusService
    {

        private readonly IEntityRepository<InstitutionStatus> repository;
        private readonly ILogger<StatusService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusService"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public StatusService(IEntityRepository<InstitutionStatus> repository, ILogger<StatusService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
        }

        public Task<InstitutionStatusDTO> Create(InstitutionStatusDTO dto)
        {
            throw new NotImplementedException();
        }

        public Task Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InstitutionStatusDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<InstitutionStatusDTO> GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Task<InstitutionStatusDTO> Update(InstitutionStatusDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}
