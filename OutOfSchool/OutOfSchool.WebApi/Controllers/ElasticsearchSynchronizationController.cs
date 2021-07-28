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
        public async Task<IActionResult> Test()
        {
            var result = await elasticsearchSynchronizationService.Synchronize();
            return Ok(result);
        }

    }
}
