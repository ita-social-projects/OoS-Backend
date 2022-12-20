using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for parent entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parents")]
public class ParentController : ControllerBase
{
    private readonly IParentService serviceParent;
    private readonly IApplicationService serviceApplication;
    private readonly IChildService serviceChild;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentController"/> class.
    /// Initialization of ParentController.
    /// </summary>
    /// <param name="serviceParent">Parent service for ParentController.</param>
    /// <param name="serviceApplication">Application service for ParentController.</param>
    /// <param name="serviceChild">Child service for ParentController.</param>
    /// <param name="mapper">Mapper.</param>
    public ParentController(
        IParentService serviceParent,
        IApplicationService serviceApplication,
        IChildService serviceChild,
        IMapper mapper)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.serviceParent = serviceParent ?? throw new ArgumentNullException(nameof(serviceParent));
        this.serviceApplication = serviceApplication ?? throw new ArgumentNullException(nameof(serviceApplication));
        this.serviceChild = serviceChild ?? throw new ArgumentNullException(nameof(serviceChild));
    }

    /// <summary>
    /// Get all Parents from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of all parents that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{ParentDTO}"/> that contains the count of all found parents and a list of parents that were received.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ParentDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] SearchStringFilter filter)
    {
        var parents = await serviceParent.GetByFilter(filter).ConfigureAwait(false);

        return Ok(parents);
    }

    /// <summary>
    /// To get information about workshops that parent applied child for.
    /// </summary>
    /// <returns>List of ParentCardDto.</returns>
    [HttpGet("childrenworkshops")]
    [HasPermission(Permissions.ParentRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ParentCard>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChildrenWorkshops()
    {
        try
        {
            var userId = GettingUserProperties.GetUserId(HttpContext);

            var children = await serviceChild.GetByUserId(userId, new OffsetFilter() {From = 0, Size = int.MaxValue})
                .ConfigureAwait(false);

            var cards = new List<ParentCard>();

            foreach (var child in children.Entities)
            {
                var applications = await serviceApplication.GetAllByChild(child.Id).ConfigureAwait(false);

                cards.AddRange(applications.Select(application => mapper.Map<ParentCard>(application)));
            }

            return Ok(cards);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    /// <summary>
    /// To recieve parent with define id.
    /// </summary>
    /// <param name="id">Key in table.</param>
    /// <returns>Parent with define id.</returns>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ParentRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await serviceParent.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// To update Parent entity that already exists.
    /// </summary>
    /// <param name="dto">ShortUserDto object with new properties.</param>
    /// <returns>Parent's key.</returns>
    [HasPermission(Permissions.ParentEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentPersonalInfo))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(ParentPersonalInfo dto)
    {
        var result = await serviceParent.Update(dto);
        return Ok(result);
    }

    /// <summary>
    /// Delete Parent entity from DB.
    /// </summary>
    /// <param name="id">The key in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.ParentRemove)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await serviceParent.Delete(id).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// To Get the Profile of authorized Parent.
    /// </summary>
    /// <returns>Authorized parent's profile.</returns>
    [HasPermission(Permissions.ParentRead)]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentDTO>> GetProfile()
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);
        var parentDto = await serviceParent.GetByUserId(userId).ConfigureAwait(false);
        return parentDto is not null ? Ok(parentDto) : NoContent();
    }
}