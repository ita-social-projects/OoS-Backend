using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PublicImageController : ControllerBase
{
    private readonly IImageService imageService;

    public PublicImageController(IImageService imageService)
    {
        this.imageService = imageService;
    }

    /// <summary>
    /// Gets <see cref="FileStreamResult"/> for a given pictureId.
    /// </summary>
    /// <param name="imageId">This is the image id.</param>
    /// <returns>The result of <see cref="FileStreamResult"/>.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{imageId}")]
    public async Task<IActionResult> GetByIdAsync(string imageId)
    {
        var imageData = await imageService.GetByIdAsync(imageId).ConfigureAwait(false);

        if (imageData.Succeeded)
        {
            return new FileStreamResult(imageData.Value.ContentStream, imageData.Value.ContentType);
        }

        return BadRequest(imageData.OperationResult);
    }
}