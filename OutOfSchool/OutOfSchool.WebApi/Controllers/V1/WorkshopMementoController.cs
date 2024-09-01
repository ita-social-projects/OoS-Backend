using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Controller with CRUD operations for IncomplitedWorkshopDto entity.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopMementoController(IMementoService<IncomplitedWorkshopDto> mementoService)
    : BaseMementoController<IncomplitedWorkshopDto>(mementoService)
{
}