using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models.SubordinationStructure;
using OutOfSchool.WebApi.Services.SubordinationStructure;

namespace OutOfSchool.WebApi.Controllers.V1.SubordinationStructure
{
    /// <summary>
    /// Controller with CRUD operations for Institution entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class InstitutionController : Controller
    {
        private readonly IInstitutionService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstitutionController"/> class.
        /// </summary>
        /// <param name="service">Service for Institution entity.</param>
        public InstitutionController(IInstitutionService service)
        {
            this.service = service;
        }

        /// <summary>
        /// To get all Institution from DB.
        /// </summary>
        /// <returns>List of all Institution, or no content.</returns>
        /// <response code="200">One or more Institution were found.</response>
        /// <response code="204">No Institution was found.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.WorkshopEdit)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<InstitutionDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var institutions = await service.GetAll().ConfigureAwait(false);

            if (!institutions.Any())
            {
                return NoContent();
            }

            return Ok(institutions);
        }
    }
}
