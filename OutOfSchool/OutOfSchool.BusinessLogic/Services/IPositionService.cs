using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Position;

namespace OutOfSchool.BusinessLogic.Services;
public interface IPositionService
{
    Task<PositionDto> CreateAsync(PositionCreateUpdateDto createDto, Guid providerId);
    Task<SearchResult<PositionDto>> GetByFilter(Guid providerId, PositionsFilter filter);
    Task<PositionDto> GetByIdAsync(Guid id, Guid providerId);
    Task<PositionDto> UpdateAsync(Guid id, PositionCreateUpdateDto updateDto, Guid providerId);
    Task DeleteAsync(Guid id, Guid providerId);
}
