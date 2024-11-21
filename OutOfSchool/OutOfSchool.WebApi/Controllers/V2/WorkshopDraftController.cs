using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V2;

[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopDraftController : ControllerBase
{
    private readonly IProviderService providerService;
    private readonly IWorkshopDraftService workshopDraftService;

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
    /// <param name="workshopDraftDto">Entity to add.</param>
    /// <returns>Created <see cref="WorkshopDraftCreateDto"/>.</returns>
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
    public async Task<IActionResult> Create([FromForm] WorkshopDraftCreateDto workshopDraftDto)
    {
        if (!ValidateWorkshopDraft(workshopDraftDto, out IActionResult validationResult))
        {
            return validationResult;
        }

        var providerValidationResult = await ValidateProvider(workshopDraftDto.ProviderId);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        var result = await workshopDraftService.Create(workshopDraftDto);

        return CreatedAtAction(
            nameof(Create),
            new { id = result.WorkshopDraft.Id },
            result);
    }

    private bool ValidateWorkshopDraft(WorkshopDraftCreateDto draft,
        out IActionResult validationResult)
    {
        validationResult = null;

        if (draft == null)
        {
            validationResult = BadRequest("The workshop draft is null.");
            return false;
        }

        if (draft.ActiveFrom > draft.ActiveTo)
        {
            ModelState.AddModelError("ActiveTo", "'ActiveFrom' must be earlier than 'ActiveTo'.");
        }

        if (!ModelState.IsValid)
        {
            validationResult = BadRequest(ModelState);
            return false;
        }

        return true;
    }

    private async Task<IActionResult> ValidateProvider(Guid providerId)
    {
        var provider = await providerService.GetById(providerId);
        if (provider == null)
        {
            return BadRequest(new { Message = $"Provider with ID {providerId} not found." });
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
