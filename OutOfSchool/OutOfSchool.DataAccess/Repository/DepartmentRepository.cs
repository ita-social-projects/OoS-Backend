using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class DepartmentRepository : SensitiveEntityRepositorySoftDeleted<Department>, IDepartmentRepository
{
    public DepartmentRepository(OutOfSchoolDbContext dbContext) : base(dbContext) { }

}
