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

    //[HasPermission(Permissions.WorkshopAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] WorkshopDraftCreateDto workshopDraft)
    {
        if (workshopDraft == null)
        {
            return StatusCode(400, "The workshop draft is null.");
        }

        var provider = await providerService.GetById(workshopDraft.ProviderId)
            .ConfigureAwait(false);

        if (provider == null)
        {
            return StatusCode(400, "The specified provider does not exist.");
        }

        if (provider.IsBlocked)
        {
            return StatusCode(403, "Forbidden to update the workshop by the blocked provider.");
        }

        var result = await workshopDraftService.Create(workshopDraft);

        return CreatedAtAction(
             nameof(Create),
             new { id = result.WorkshopDraft.Id, },
             result);
    }
}
