using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller for Language entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class LanguageController : ControllerBase
{
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageController"/> class.
    /// </summary>
    /// <param name="languageService">Service for Language entity.</param>
    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <summary>
    /// Gets all language entities from the database.
    /// </summary>
    /// <returns>List of Languages.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LanguageDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var languages = await _languageService.GetAll().ConfigureAwait(false);

        if (!languages.Any())
        {
            return NoContent();
        }

        return Ok(languages);
    }
}
