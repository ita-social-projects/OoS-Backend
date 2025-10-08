using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;
using OutOfSchool.BusinessLogic.Services.TempSave;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>The abstract controller with operations for temporarily saving a CompetitiveEvent in cache when it is created.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.CompetitiveEventAddNew)]

public class CompetitiveEventTempSaveController(ITempSaveService<CompetitiveEventAboutDto> tempSaveService)
    : TempSaveController<CompetitiveEventAboutDto>(tempSaveService)
{
}