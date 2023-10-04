using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ChildRepository : EntityRepositorySoftDeleted<Guid, Child>, IEntityRepository<Guid, Child>
{
    private readonly OutOfSchoolDbContext db;

    public ChildRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public override Task<Child> Create(Child child)
    {
        return base.Create(child);
    }
}