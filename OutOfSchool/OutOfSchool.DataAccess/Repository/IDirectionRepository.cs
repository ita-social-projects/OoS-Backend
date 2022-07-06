using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IDirectionRepository : IEntityRepository<Direction>
{
    Task<IEnumerable<Direction>> GetPagedByFilter(int skip, int take, Expression<Func<Direction, bool>> predicate);
}