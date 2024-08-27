using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
using OutOfSchool.BusinessLogic.Services.Memento.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>Controller with CRUD operations for RequiredWorkshopMemento entity.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopMementoController : BaseMementoController<RequiredWorkshopMemento>
{
    /// <summary>Initializes a new instance of the <see cref="WorkshopMementoController" /> class.</summary>
    /// <param name="mementoService">The memento service.</param>
    /// <param name="storage">The storage.</param>
    public WorkshopMementoController(
        IMementoService<RequiredWorkshopMemento> mementoService,
        IStorage storage)
        : base(mementoService, storage)
    {
    }
}
