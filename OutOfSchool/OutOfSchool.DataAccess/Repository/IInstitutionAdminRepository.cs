using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IInstitutionAdminRepository : IEntityRepository<long, InstitutionAdmin>
{
    Task<InstitutionAdmin> GetByIdAsync(string userId);
}