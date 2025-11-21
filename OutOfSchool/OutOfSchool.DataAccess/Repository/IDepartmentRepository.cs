using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository;

public interface IDepartmentRepository : ISensitiveEntityRepositorySoftDeleted<Department>
{
}
