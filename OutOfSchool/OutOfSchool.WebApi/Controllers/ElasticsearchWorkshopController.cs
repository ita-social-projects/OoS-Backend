using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "admin")]
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
        public async Task<IActionResult> Search([FromQuery] WorkshopFilterDto filter)
        {
            var res = await esService.Search(filter.ToESModel()).ConfigureAwait(false);

            return Ok(res);
        }
    }
}
