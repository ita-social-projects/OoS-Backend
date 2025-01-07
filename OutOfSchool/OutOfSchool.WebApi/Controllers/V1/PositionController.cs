using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Position;

namespace OutOfSchool.WebApi.Controllers.V1;
[Route("api/v{version:apiVersion}/providers/{providerId}/positions/[action]")]
[Authorize(Roles = "provider")]
[ApiController]
public class PositionController : ControllerBase
{
    private readonly IPositionService positionService;

    public PositionController(IPositionService positionService)
    {
        this.positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
    }

    /// <summary>
    /// Creates a new position for the current provider.
    /// </summary>
    /// <param name="createDto">The position data to create.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <returns>The created position.</returns>
    [HttpPost]  
    [HasPermission(Permissions.PositionAddNew)]
    public async Task<IActionResult> Create(Guid providerId, [FromBody] PositionCreateUpdateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (createDto == null)
            {
                return BadRequest();
            }            
            
            var createdPosition = await positionService.CreateAsync(createDto, providerId).ConfigureAwait(false);

            return CreatedAtAction(
            nameof(GetById),
            new { positionId = createdPosition.Id, providerId = providerId },
            createdPosition
            );
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all positions for the provider with filters.
    /// </summary>
    /// <param name="providerId">The ID of provider.</param>
    /// <param name="filter">Pamrameters to filter the position</param>
    /// <returns><see cref="SearchResult{PositionDto}"/>.</returns>
    [HttpGet]
    [HasPermission(Permissions.PositionRead)]
    public async Task<IActionResult> GetByFilter(Guid providerId, [FromQuery] PositionsFilter filter)
    {                
        var positions = await positionService.GetByFilter(providerId, filter);
        return positions.TotalAmount == 0 ? 
            this.Ok("There is no records for given provider") : 
            this.SearchResultToOkOrNoContent(positions);
    }

    /// <summary>
    /// Retrieves a specific position by its ID.
    /// </summary>    
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="positionId">The ID of the position to get.</param>
    /// <returns>The position details.</returns>
    [HttpGet("{positionId}")]
    [HasPermission(Permissions.PositionRead)]
    public async Task<IActionResult> GetById(Guid providerId, Guid positionId)
    {
        try 
        {
            var position = await positionService.GetByIdAsync(positionId, providerId).ConfigureAwait(false);
            return Ok(position);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }                
    }

    /// <summary>
    /// Updates an existing position.
    /// </summary>    
    /// <param name="updateDto">The updated position data.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="positionId">The ID of the position to update.</param>
    /// <returns>The updated position.</returns>    
    [HttpPut("{positionId}")]
    [HasPermission(Permissions.PositionEdit)]
    public async Task<IActionResult> Update([FromBody] PositionCreateUpdateDto updateDto, Guid providerId, Guid positionId)
    {
        try 
        {          
            var updatedPosition = await positionService.UpdateAsync(positionId, updateDto, providerId);
            return Ok(updatedPosition);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a specific position.
    /// </summary>    
    /// <param name="providerId">The ID of the provider.</param>
    /// <param name="positionId">The ID of the position to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{positionId}")]
    [HasPermission(Permissions.PositionRemove)]
    public async Task<IActionResult> Delete(Guid providerId, Guid positionId)
    {
        try
        {
            await positionService.DeleteAsync(positionId, providerId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }        
    }
}