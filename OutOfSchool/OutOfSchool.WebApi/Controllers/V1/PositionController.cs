using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[ApiController]
public class PositionController : ControllerBase
{
    private readonly IPositionService positionService;
    private readonly ICurrentUserService currentUserService;
    private readonly IUserService userService;
    private readonly IProviderService providerService;

    public PositionController(IPositionService positionService, ICurrentUserService currentUserService, IUserService userService, IProviderService providerService)
    {
        this.positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));        
        this.userService = userService ?? throw new ArgumentNullException( nameof(userService));
    }

    [HttpGet]
    [Route("CurrentProviderId")]
    public IActionResult CurrentProviderId() => Ok(GetProviderId());

    /// <summary>
    /// Create a new position for the current provider.
    /// </summary>
    [HttpPost]  
    public async Task<ActionResult<PositionDto>> Create([FromBody] PositionCreateDto createDto)
    {
        if (createDto == null)
        {
            return BadRequest();
        }
        var providerId = GetProviderId();
        var createdPosition = await positionService.CreateAsync(createDto, providerId);
        return CreatedAtAction(nameof(GetById), new { id = createdPosition.Id }, createdPosition);
    }

    /// <summary>
    /// Get all positions for current provider.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll()
    {
        try 
        {
            var providerId = GetProviderId();
            var positions = await positionService.GetAllAsync(providerId);
            return Ok(positions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Get a specific position by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PositionDto>> GetById(Guid id)
    {
        try 
        {
            var position = await positionService.GetByIdAsync(id);
            return Ok(position);
        }        
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing position.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PositionDto>> Update(Guid id, [FromBody] PositionUpdateDto updateDto)
    {
        try 
        {
            var providerId = GetProviderId();
            var updatedPosition = await positionService.UpdateAsync(id, updateDto, providerId); // positionId, dto, user who owns position Provider
            return Ok(updatedPosition);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a position.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var providerId = GetProviderId();
            await positionService.DeleteAsync(id, providerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    // Method to extract provider user ID
    private Guid GetProviderId()
    {
        var justId = currentUserService.UserId;
        if (currentUserService.IsInRole(Role.Provider))
        {
            var id = currentUserService.UserId;
            return Guid.Parse(id);
        }
               
        return Guid.Empty;        
    }
}
