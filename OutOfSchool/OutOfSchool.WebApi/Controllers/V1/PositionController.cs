using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Position;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "provider")]
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
    

    /// <summary>
    /// Creates a new position for the current provider.
    /// </summary>
    /// <param name="createDto">The position data to create.</param>
    /// <returns>The created position.</returns>
    [HttpPost]  
    public async Task<ActionResult<PositionDto>> Create([FromBody] PositionCreateDto createDto)
    {
        try
        {
            if (createDto == null)
            {
                return BadRequest();
            }
            var providerOwnerId = GetProviderOwnerId();
            var createdPosition = await positionService.CreateAsync(createDto, providerOwnerId);
            return CreatedAtAction(nameof(GetById), new { id = createdPosition.Id }, createdPosition);
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all positions for the current provider.
    /// </summary>
    /// <returns>List of positions.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAll()
    {
        try 
        {
            var providerOwnerId = GetProviderOwnerId();
            var positions = await positionService.GetAllAsync(providerOwnerId);
            return Ok(positions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Retrieves a specific position by its ID.
    /// </summary>
    /// <param name="id">The ID of the position.</param>
    /// <returns>The position details.</returns>
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
    /// Updates an existing position.
    /// </summary>
    /// <param name="id">The ID of the position to update.</param>
    /// <param name="updateDto">The updated position data.</param>
    /// <returns>The updated position.</returns>    
    [HttpPut("{id}")]
    public async Task<ActionResult<PositionDto>> Update(Guid id, [FromBody] PositionUpdateDto updateDto)
    {
        try 
        {
            var providerOwnerId = GetProviderOwnerId();
            var updatedPosition = await positionService.UpdateAsync(id, updateDto, providerOwnerId); // positionId, dto, user who owns position Provider
            return Ok(updatedPosition);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a specific position.
    /// </summary>
    /// <param name="id">The ID of the position to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var providerOwnerId = GetProviderOwnerId();
            await positionService.DeleteAsync(id, providerOwnerId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); 
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    // Method to extract provider user ID
    private Guid GetProviderOwnerId()
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
