using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Controller with CRUD operations for WorkshopWithRequiredPropertiesDto entity.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopMementoController(IMementoService<WorkshopWithRequiredPropertiesDto> mementoService)
    : BaseMementoController<WorkshopWithRequiredPropertiesDto>(mementoService)
{
}