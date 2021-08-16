using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    //[Authorize(Roles = "admin")]
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
    }
}
