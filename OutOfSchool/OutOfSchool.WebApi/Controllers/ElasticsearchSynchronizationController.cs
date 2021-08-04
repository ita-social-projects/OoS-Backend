using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ElasticsearchSynchronizationController : ControllerBase
    {
        private readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService;

        public ElasticsearchSynchronizationController(IElasticsearchSynchronizationService elasticsearchSynchronizationService)
        {
            this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Synchronize()
        {
            var result = await elasticsearchSynchronizationService.Synchronize().ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await elasticsearchSynchronizationService.GetAll().ConfigureAwait(false);

            return Ok(result);
        }
    }
}
