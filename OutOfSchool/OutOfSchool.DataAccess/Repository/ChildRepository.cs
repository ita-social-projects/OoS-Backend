using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ChildRepository : EntityRepository<Guid, Child>, IEntityRepository<Guid, Child>
{
    private readonly OutOfSchoolDbContext db;

    public ChildRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public override Task<Child> Create(Child child)
    {
        db.SocialGroups.AttachRange(child.SocialGroups);
        return base.Create(child);
    }
}