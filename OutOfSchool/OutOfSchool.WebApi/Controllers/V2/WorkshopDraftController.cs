using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>
/// Controller with CRUD operations for WorkshopDraft entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopDraftController : ControllerBase
{
    private readonly IProviderService providerService;
    private readonly IWorkshopDraftService workshopDraftService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopDraftController"/> class.
    /// </summary>
    /// <param name="providerService">Service for Provider model</param>
    /// <param name="workshopDraftService">Service for WorkshopDraft model.</param>

    public WorkshopDraftController(
        IProviderService providerService,
        IWorkshopDraftService workshopDraftService)
    {
        this.providerService = providerService;
        this.workshopDraftService = workshopDraftService;
    }


    /// <summary>
    /// Add new workshop draft to the database.
    /// </summary>
    /// <param name="workshopV2Dto">Entity to add.</param>
    /// <returns>Created <see cref="WorkshopDraftResultDto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDraftResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] WorkshopV2Dto workshopV2Dto)
    {
        var providerValidationResult = await ValidateProvider(workshopV2Dto.ProviderId);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        // TODO: After implementing the new workshop model, add validation to check if the parent workshop is nested.
        // A workshop must have only one level of nesting.

        var result = await workshopDraftService.Create(workshopV2Dto);

        return CreatedAtAction(
            nameof(Create),
            new { id = result.WorkshopDraft.Id },
            result);
    }
    
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] WorkshopDraftUpdateDto workshopDraftUpdateDto)
    {
        var providerValidationResult = await ValidateProvider(workshopDraftUpdateDto.WorkshopV2Dto.ProviderId);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        try
        {
            var result = await workshopDraftService.Update(workshopDraftUpdateDto);
            return Ok(result);
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HasPermission(Permissions.WorkshopRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await workshopDraftService.Delete(id);
            return NoContent();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> SendForModeration(Guid id)
    {
        try
        {
            await workshopDraftService.SendForModeration(id);
            return Ok();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string rejectionMessage)
    {
        if (string.IsNullOrWhiteSpace(rejectionMessage))
        {
            return BadRequest("RejectionMessage can`t be empty");
        }

        try
        {
            await workshopDraftService.Reject(id, rejectionMessage);
            return Ok();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
          
    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await workshopDraftService.Approve(id);
            return Ok();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopDraftResponseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter) =>
        await workshopDraftService.GetByProviderId(id, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);
    
    private async Task<IActionResult> ValidateProvider(Guid providerId)
    {
        var provider = await providerService.GetById(providerId);
        if (provider == null)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new { Message = $"Provider with ID {providerId} not found." });
        }

        if (provider.IsBlocked)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { Message = $"Provider with ID {providerId} is blocked and cannot create workshop drafts." });
        }

        return null;
    }
}
