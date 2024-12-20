using OutOfSchool.BusinessLogic.Models.Position;

namespace OutOfSchool.BusinessLogic.Services;
public interface IPositionService
{
    Task<PositionDto> CreateAsync(PositionCreateDto createDto, Guid providerId);
    Task<IEnumerable<PositionDto>> GetAllAsync(Guid id);
    Task<PositionDto> GetByIdAsync(Guid id);
    Task<PositionDto> UpdateAsync(Guid id, PositionUpdateDto updateDto, Guid providerId);
    Task DeleteAsync(Guid id, Guid providerId);
}
