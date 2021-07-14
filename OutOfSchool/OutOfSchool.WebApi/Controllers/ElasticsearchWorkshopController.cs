using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
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
        public async Task Delete(long id)
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
