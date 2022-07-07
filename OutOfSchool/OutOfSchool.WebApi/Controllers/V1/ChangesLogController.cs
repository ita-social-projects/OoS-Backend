using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for ChangesLog entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ChangesLogController : ControllerBase
{
    private readonly IChangesLogService changesLogService;

    public ChangesLogController(IChangesLogService changesLogService)
    {
        this.changesLogService = changesLogService;
    }

    /// <summary>
    /// Get history of Provider changes that matches filter's parameters.
    /// </summary>
    /// <param name="request">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{ProviderChangesLogDto}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given filter.</response>
    /// <response code="204">No entity with given filter was found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderChangesLogDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Provider([FromQuery] ProviderChangesLogRequest request)
    {
        var changesLog = await changesLogService.GetProviderChangesLogAsync(request).ConfigureAwait(false);

        if (changesLog.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(changesLog);
    }

    /// <summary>
    /// Get history of Application changes that matches filter's parameters.
    /// </summary>
    /// <param name="request">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{ApplicationChangesLogDto}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given filter.</response>
    /// <response code="204">No entity with given filter was found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ApplicationChangesLogDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Application([FromQuery] ApplicationChangesLogRequest request)
    {
        var changesLog = await changesLogService.GetApplicationChangesLogAsync(request).ConfigureAwait(false);

        if (changesLog.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(changesLog);
    }

    /// <summary>
    /// Get history of ProviderAdmin changes that matches filter's parameters.
    /// </summary>
    /// <param name="request">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{ProviderAdminChangesLogDto}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given filter.</response>
    /// <response code="204">No entity with given filter was found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderAdminChangesLogDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProviderAdmin([FromQuery] ProviderAdminChangesLogRequest request)
    {
        var changesLog = await changesLogService.GetProviderAdminChangesLogAsync(request).ConfigureAwait(false);

        if (changesLog.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(changesLog);
    }
}