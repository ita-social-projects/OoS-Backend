using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ThumbnailTestController : ControllerBase
{
    private readonly IThumbnailProcessingService thumbnailService;
    private readonly ILogger<ThumbnailTestController> logger;

    public ThumbnailTestController(
        IThumbnailProcessingService thumbnailService,
        ILogger<ThumbnailTestController> logger)
    {
        this.thumbnailService = thumbnailService;
        this.logger = logger;
    }

    [HttpPost("{imageId}")]
    public async Task<IActionResult> GenerateThumbnail(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest("Image ID is required.");
        }

        try
        {
            var result = await thumbnailService.ProcessImage(imageId);

            if (!result)
            {
                return StatusCode(500, "Thumbnail generation failed.");
            }

            return Ok($"Thumbnail for imageId '{imageId}' generated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating thumbnail for imageId: {ImageId}", imageId);
            return StatusCode(500, "Internal error occurred during thumbnail generation.");
        }
    }

    [HttpPost("{imageId}")]
    public async Task<IActionResult> HasThumbnail(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest("Image ID is required.");
        }

        try
        {
            var result = await thumbnailService.HasThumbnail(imageId);

            if (!result)
            {
                return StatusCode(500, "Thumbnail not exist.");
            }

            return Ok($"Thumbnail for imageId '{imageId}' is alrady exist in external storage.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occures while searching for thumbnail with imageId: {ImageId}", imageId);
            return StatusCode(500, "Internal error occurred during thumbnail searching.");
        }
    }
}
