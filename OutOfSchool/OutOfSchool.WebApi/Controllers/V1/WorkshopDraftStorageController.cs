using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Controller with operations for storing the Teacher draft in cache.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopDraftStorageController(IDraftStorageService<WorkshopWithRequiredPropertiesDto> mementoService)
    : DraftStorageController<WorkshopWithRequiredPropertiesDto>(mementoService)
{
}