using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository;
public class QuartzJobRepository : EntityRepository<long, QuartzJob>, IQuartzJobRepository
{
    public QuartzJobRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
