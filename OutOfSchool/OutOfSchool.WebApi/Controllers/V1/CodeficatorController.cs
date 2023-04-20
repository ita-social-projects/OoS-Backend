using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models.Codeficator;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CodeficatorController : ControllerBase
{
    private readonly ICodeficatorService codeficatorService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeficatorController"/> class.
    /// </summary>
    /// <param name="codeficatorService">Service for Codeficator model.</param>
    public CodeficatorController(
        ICodeficatorService codeficatorService)
    {
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
    }

    /// <summary>
    /// Get only the Id and Name of the child entities as a list from the database.
    /// </summary>
    /// <param name="id"> Parent's id. If null will be return pair values for the main level (with Category equal O or K). </param>
    /// <returns> List of pair values of child entities.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<KeyValuePair<long, string>>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("children/names")]
    public async Task<IActionResult> GetNames(long? id)
    {
        var regions = await codeficatorService.GetChildrenNamesByParentId(id).ConfigureAwait(false);

        if (!regions.Any())
        {
            return NoContent();
        }

        return Ok(regions);
    }

    /// <summary>
    /// Get list of chaild entities from the database.
    /// </summary>
    /// <param name="id"> Parent's id. If null will be return entities for the main level (with Category equal O or K). </param>
    /// <returns> List of chaild entities.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CodeficatorDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("children")]
    public async Task<IActionResult> Get(long? id)
    {
        var regions = await codeficatorService.GetChildrenByParentId(id).ConfigureAwait(false);

        if (!regions.Any())
        {
            return NoContent();
        }

        return Ok(regions);
    }

    /// <summary>
    /// Get all address's parts from the database.
    /// </summary>
    /// <param name="id"> Codeficator's id. </param>
    /// <returns> All address parts for the codeficator. </returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllAddressPartsDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}/parents")]
    public async Task<IActionResult> GetParents(long id)
    {
        var addressParts = await codeficatorService.GetAllAddressPartsById(id).ConfigureAwait(false);

        if (addressParts == null)
        {
            return NoContent();
        }

        return Ok(addressParts);
    }

    /// <summary>
    /// Get all address's parts from the database.
    /// </summary>
    /// <param name="filter">Filter for the search.</param>
    /// <returns> All address parts for the codeficator. </returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CodeficatorAddressDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("search")]
    public async Task<IActionResult> SerchByPartOfName([FromQuery] CodeficatorFilter filter)
    {
        if (filter == null)
        {
            return BadRequest("CodeficatorFilter is null.");
        }

        if (!string.IsNullOrEmpty(filter.Name) && filter.Name.Length < 3)
        {
            return BadRequest("The field Name must be a string type with a minimum length of '3'.");
        }

        var fullAddressNames = await codeficatorService.GetFullAddressesByPartOfName(filter).ConfigureAwait(false);

        if (!fullAddressNames.Any())
        {
            return NoContent();
        }

        return Ok(fullAddressNames);
    }

    /// <summary>
    /// Get nearest settlement to coordinates.
    /// </summary>
    /// <param name="query">Coordinates query.</param>
    /// <returns> The codeficator. </returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CodeficatorAddressDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("[action]")]
    public async Task<IActionResult> NearestByCoordinates([FromQuery] NearestCodeficatorRequest query)
    {
        if (query == null)
        {
            return BadRequest("NearestCodeficatorRequest is null.");
        }

        var codeficator = await codeficatorService.GetNearestByCoordinates(query.Lat, query.Lon);

        return codeficator is not null ? Ok(codeficator) : NoContent();
    }
}