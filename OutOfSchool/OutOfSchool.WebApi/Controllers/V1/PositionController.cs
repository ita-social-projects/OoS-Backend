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
    //[HasPermission(Permissions.PositionAddNew)]
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
    //[HasPermission(Permissions.PositionRead)]
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
    /// <param name="positionId">The ID of the position to get.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <returns>The position details.</returns>
    [HttpGet("{positionId}")]
    //[HasPermission(Permissions.PositionRead)]
    public async Task<IActionResult> GetById(Guid positionId, Guid providerId)
    {        
        return Ok(await positionService.GetByIdAsync(positionId, providerId).ConfigureAwait(false));       
    }

    /// <summary>
    /// Updates an existing position.
    /// </summary>
    /// <param name="positionId">The ID of the position to update.</param>
    /// <param name="updateDto">The updated position data.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <returns>The updated position.</returns>    
    [HttpPut("{positionId}")]
    //[HasPermission(Permissions.PositionEdit)]
    public async Task<IActionResult> Update(Guid positionId, [FromBody] PositionCreateUpdateDto updateDto, Guid providerId)
    {
        try 
        {          
            var updatedPosition = await positionService.UpdateAsync(positionId, updateDto, providerId);
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
    /// <param name="positionId">The ID of the position to delete.</param>
    /// <param name="providerId">The ID of the provider.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{positionId}")]
    //[HasPermission(Permissions.PositionRemove)]
    public async Task<IActionResult> Delete(Guid positionId, Guid providerId)
    {
        try
        {            
            await positionService.DeleteAsync(positionId, providerId);
            return NoContent();
        }        
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
