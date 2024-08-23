using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with operations for RequiredWorkshopMemento entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopMementoController : BaseMementoController<RequiredWorkshopMemento>
{
    public WorkshopMementoController(
        IRedisCacheService redisCacheService, 
        IMementoService<RequiredWorkshopMemento> mementoService, 
        IStorage storage)
        : base(redisCacheService, mementoService, storage)
    {
    }
}
