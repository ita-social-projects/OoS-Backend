using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ChildRepository : EntityRepository<Child>, IEntityRepository<Child>
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
}
