using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class CodeficatorRepository : EntityRepository<Codeficator>, ICodeficatorRepository
    {
        private readonly OutOfSchoolDbContext db;

        public CodeficatorRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeyValuePair<long, string>>> GetNamesByFilter(Expression<Func<Codeficator, bool>> predicate)
        {
            IQueryable<KeyValuePair<long, string>> query = db.Codeficators
                .Where(predicate)
                .OrderBy(x => x.Name)
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Name));

            return await query.ToListAsync().ConfigureAwait(false);
        }
    }
}