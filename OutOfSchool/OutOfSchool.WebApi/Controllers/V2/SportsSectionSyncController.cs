using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Services.SportsRegistry;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
namespace OutOfSchool.WebApi.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/sports-sections/sync")]
    public class SportsSectionSyncController : ControllerBase
    {
        private readonly ISportsSectionSyncService syncService;
        private readonly ISportsRegistryWorkshopProvider workshopProvider;
        private readonly IWorkshopDraftRepository workshopDraftRepository;
        private readonly ILogger<SportsSectionSyncController> logger;
        public SportsSectionSyncController(
            ISportsSectionSyncService syncService,
            ISportsRegistryWorkshopProvider workshopProvider,
            IWorkshopDraftRepository workshopDraftRepository,
            ILogger<SportsSectionSyncController> logger)
        {
            this.syncService = syncService;
            this.workshopProvider = workshopProvider;
            this.workshopDraftRepository = workshopDraftRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Виконує синхронізацію спортивних секцій із Мінспорту до Позашкілля.
        /// </summary>
        /// <returns>Кількість створених або оновлених секцій.</returns>
        [HttpPost("run")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> RunSyncAsync()
        {
            try
            {
                logger.LogInformation("Manual sync of sports sections started...");

                var result = await syncService.SyncSportsSectionsAsync().ConfigureAwait(false);

                logger.LogInformation("Manual sync completed. {Count} sections processed.", result);

                return Ok(new
                {
                    message = "Sync completed successfully.",
                    totalProcessed = result
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while syncing sports sections.");
                return StatusCode(500, new
                {
                    message = "An error occurred during synchronization.",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Показує поточний стан секцій у Мінспорту та Позашкіллі (для перевірки перед синхронізацією).
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatusAsync()
        {
            try
            {
                logger.LogInformation("Fetching sports sections status...");

                var externalResponse = await workshopProvider.GetAllSportsSectionsAsync().ConfigureAwait(false);

                var externalSections = externalResponse.Match(
                    error => throw new InvalidOperationException($"Failed to fetch sports sections: {error.Message}"),
                    success => success);

                var localDrafts = await workshopDraftRepository.GetAll().ConfigureAwait(false);

                var totalExternal = externalSections.Count;
                var totalLocal = localDrafts.Count();
                var linked = localDrafts.Count(w => w.MinsportSectionId.HasValue);
                var notLinked = totalLocal - linked;

                return Ok(new
                {
                    external = totalExternal,
                    local = totalLocal,
                    linked,
                    notLinked,
                    message = "Sports sections status retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while fetching sports sections status.");
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching status.",
                    details = ex.Message
                });
            }
        }
    }
}
