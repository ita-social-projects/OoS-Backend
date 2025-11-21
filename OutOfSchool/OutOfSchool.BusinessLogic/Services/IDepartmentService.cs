using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Department;

namespace OutOfSchool.BusinessLogic.Services;

public interface IDepartmentService
{
    Task<DepartmentDto> CreateAsync(DepartmentCreateUpdateDto createDto, Guid providerId);
    Task<SearchResult<DepartmentDto>> GetByFilter(Guid providerId, DepartmentFilter filter);
    Task<DepartmentDto> GetByIdAsync(Guid id, Guid providerId);
    Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateUpdateDto updateDto, Guid providerId);
    Task DeleteAsync(Guid id, Guid providerId);
}
