using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class CodeficatorRepository : EntityRepository<long, CATOTTG>, ICodeficatorRepository
{
    private readonly OutOfSchoolDbContext db;

    public CodeficatorRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<KeyValuePair<long, string>>> GetNamesByFilter(Expression<Func<CATOTTG, bool>> predicate)
    {
        IQueryable<KeyValuePair<long, string>> query = db.CATOTTGs
            .Where(predicate)
            .OrderBy(x => x.Name)
            .Select(x => new KeyValuePair<long, string>(x.Id, x.Name));

        return await query.ToListAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<CodeficatorAddressDto>> GetFullAddressesByPartOfName(string namePart, string categories = default)
    {
        var query2 = from e in db.CATOTTGs
                     from p in db.CATOTTGs.Where(x1 => e.ParentId == x1.Id).DefaultIfEmpty()
                     from pp in db.CATOTTGs.Where(x2 => p.ParentId == x2.Id).DefaultIfEmpty()
                     from ppp in db.CATOTTGs.Where(x3 => pp.ParentId == x3.Id).DefaultIfEmpty()
                     where e.Name.StartsWith(namePart) && categories.Contains(e.Category)
                     select new CodeficatorAddressDto
                     {
                         Id = e.Id,
                         Category = e.Category,
                         Settlement = e.Name,
                         Latitude = e.Latitude,
                         Longitude = e.Longitude,
                         TerritorialCommunity = p.Name,
                         District = pp.Name,
                         Region = ppp.Name,
                     };

        return await query2.ToListAsync();
    }
}