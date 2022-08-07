using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class DirectionRepository : EntityRepository<long, Direction>, IDirectionRepository
{
    public DirectionRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IEnumerable<Direction>> GetPagedByFilter(int skip, int take, Expression<Func<Direction, bool>> predicate)
    {
        return await dbSet
            .OrderBy(direction => direction.Id)
            .Where(predicate)
            .Skip(skip)
            .Take(take)
            .ToListAsync()
            .ConfigureAwait(false);
    }
}