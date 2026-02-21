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

    /// <summary>
    /// Gets a language by its ID.
    /// </summary>
    /// <param name="id">Language ID.</param>
    /// <returns>Language DTO if found, NotFound if not.</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LanguageDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var language = await _languageService.GetById(id).ConfigureAwait(false);

        if (language is null)
        {
            return NotFound();
        }

        return Ok(language);
    }
}
