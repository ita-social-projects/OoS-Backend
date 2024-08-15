using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository;

public class ChildRepository : EntityRepositorySoftDeleted<Guid, Child>, IEntityRepository<Guid, Child>
{
    public ChildRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task<Child> Create(Child child)
    {
        return base.Create(child);
    }
}