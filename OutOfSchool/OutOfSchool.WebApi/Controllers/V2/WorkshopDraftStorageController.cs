using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
using OutOfSchool.BusinessLogic.Services.DraftStorage;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>Controller with operations for storing the Workshop draft in cache.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopDraftStorageController(IDraftStorageService<WorkshopMainRequiredPropertiesDto> draftStorageService)
    : DraftStorageController<WorkshopMainRequiredPropertiesDto>(draftStorageService)
{
}