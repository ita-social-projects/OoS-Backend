using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IInstitutionAdminRepository : IEntityRepository<(string, Guid), InstitutionAdmin>
{
    Task<InstitutionAdmin> GetByIdAsync(string userId);
}