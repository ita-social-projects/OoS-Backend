using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HasPermission(Permissions.SystemManagement)]
    public class ElasticsearchWorkshopController : ControllerBase
    {
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> esService;

        public ElasticsearchWorkshopController(IElasticsearchService<WorkshopES, WorkshopFilterES> esService)
        {
            this.esService = esService;
        }

        [HttpPost]
        public async Task Add(WorkshopES entity)
        {
            await esService.Index(entity).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task Update(WorkshopES entity)
        {
            await esService.Update(entity).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task Delete(Guid id)
        {
            await esService.Delete(id).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task ReIndex()
        {
            await esService.ReIndex().ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] WorkshopFilterES filter)
        {
            var res = await esService.Search(filter).ConfigureAwait(false);

            return Ok(res);
        }
    }
}
