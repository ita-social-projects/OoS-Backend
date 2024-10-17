using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.SystemManagement)]
public class ElasticsearchWorkshopController : ControllerBase
{
    private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> esService;

    public ElasticsearchWorkshopController(IElasticsearchService<WorkshopES, WorkshopFilterES> esService)
    {
        this.esService = esService;
    }

    [Obsolete("This method is obsolete and will be removed in a future version")]
    [HttpPost]
    public async Task Add(WorkshopES entity)
    {
        await esService.Index(entity).ConfigureAwait(false);
    }

    [Obsolete("This method is obsolete and will be removed in a future version")]
    [HttpPut]
    public async Task Update(WorkshopES entity)
    {
        await esService.Update(entity).ConfigureAwait(false);
    }

    [Obsolete("This method is obsolete and will be removed in a future version")]
    [HttpDelete]
    public async Task Delete(Guid id)
    {
        await esService.Delete(id).ConfigureAwait(false);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<StatusCodeResult> ReIndex()
    {
        var res = await esService.ReIndex().ConfigureAwait(false);

        if (res)
        {
            return StatusCode(StatusCodes.Status200OK);
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResultES<WorkshopES>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Search([FromQuery] WorkshopFilterES filter)
    {
        var res = await esService.Search(filter).ConfigureAwait(false);

        return Ok(res);
    }
}