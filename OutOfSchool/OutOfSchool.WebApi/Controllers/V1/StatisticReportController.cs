using Google.Api;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Notifications;
using OutOfSchool.WebApi.Models.StatisticReports;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class StatisticReportController : ControllerBase
{
    private readonly IStatisticReportService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticReportController"/> class.
    /// </summary>
    /// <param name="statisticReportService">Service for StatisticReport model.</param>
    public StatisticReportController(IStatisticReportService statisticReportService)
    {
        this.service = statisticReportService;
    }

    /// <summary>
    /// Get all statistic reports from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of statistic reports that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{StatisticReportDto}"/> that contains the count of all found statistic reports and a list of statistic reports that were received.</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<StatisticReportDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetByFilter([FromQuery] StatisticReportFilter filter)
    {
        return Ok(await service.GetByFilter(filter).ConfigureAwait(false));
    }

    /// <summary>
    /// Get StatisticReport data by it's id.
    /// </summary>
    /// <param name="externalId">StatisticReport id.</param>
    /// <returns>Notification.</returns>
    /// <response code="200">Returns StatisticReport.</response>
    /// <response code="400">Id was wrong.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any server error occures.</response>
    [Authorize]
    [HttpGet("{externalId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatisticReportDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDataById(string externalId)
    {
        var fileData = await service.GetDataById(externalId).ConfigureAwait(false);

        return new FileStreamResult(fileData.ContentStream, fileData.ContentType);
    }
}
