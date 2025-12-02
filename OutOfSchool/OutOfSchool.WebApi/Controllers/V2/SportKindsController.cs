using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;

namespace OutOfSchool.WebApi.Controllers;

/// <summary>
/// Controller for testing synchronization of sport kinds with the external Sports Registry.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SportKindsController : ControllerBase
{
    private readonly ISportKindSyncService syncService;

    public SportKindsController(ISportKindSyncService syncService)
    {
        this.syncService = syncService;
    }

    /// <summary>
    /// Triggers synchronization of sport kinds from Sports Registry.
    /// </summary>
    /// <returns>JSON with number of created/updated records.</returns>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var updated = await syncService.SyncSportKindsAsync().ConfigureAwait(false);
        return Ok(new
        {
            message = "Synchronization completed",
            updatedRecords = updated
        });
    }
}