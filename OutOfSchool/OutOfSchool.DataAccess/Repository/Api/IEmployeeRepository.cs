using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IEmployeeRepository : IEntityRepositorySoftDeleted<(string, Guid), Employee>
{
    Task<bool> IsExistEmployeeWithUserIdAsync(Guid providerId, string userId);

    Task<bool> IsExistProviderWithUserIdAsync(string userId);

    Task<Provider> GetProviderWithUserIdAsync(string userId);

    Task<int> GetNumberEmployeesAsync(Guid providerId);

    Task AddRelatedWorkshopForEmployee(string userId, Guid workshopId);

    Task<Employee?> GetByIdAsync(string userId, Guid providerId);
}