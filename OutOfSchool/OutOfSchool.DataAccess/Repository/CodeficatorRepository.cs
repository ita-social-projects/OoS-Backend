using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using H3Lib;
using H3Lib.Extensions;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
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
        var query = from e in db.CATOTTGs
                     from p in db.CATOTTGs.Where(x1 => e.ParentId == x1.Id).DefaultIfEmpty()
                     from pp in db.CATOTTGs.Where(x2 => p.ParentId == x2.Id).DefaultIfEmpty()
                     from ppp in db.CATOTTGs.Where(x3 => pp.ParentId == x3.Id).DefaultIfEmpty()
                     from pppp in db.CATOTTGs.Where(x4 => ppp.ParentId == x4.Id).DefaultIfEmpty()
                     where ((CodeficatorCategory.Level4.Name.Contains(e.Category) && e.Name.StartsWith(namePart)) || (e.Category == CodeficatorCategory.CityDistrict.Name && p.Name.StartsWith(namePart))) && categories.Contains(e.Category)
                     select new CodeficatorAddressDto
                     {
                         Id = e.Id,
                         Category = e.Category,
                         Settlement = e.Category == CodeficatorCategory.CityDistrict.Name ? p.Name : e.Name,
                         Latitude = e.Latitude,
                         Longitude = e.Longitude,
                         Order = e.Order,
                         TerritorialCommunity = e.Category == CodeficatorCategory.CityDistrict.Name ? pp.Name : p.Name,
                         District = e.Category == CodeficatorCategory.CityDistrict.Name ? ppp.Name : pp.Name,
                         Region = e.Category == CodeficatorCategory.CityDistrict.Name ? pppp.Name : ppp.Name,
                         CityDistrict = e.Category == CodeficatorCategory.CityDistrict.Name ? e.Name : null,
                     };

        return await query.OrderBy(x => x.Order).ToListAsync();
    }

    public async Task<CATOTTG> GetNearestByCoordinates(double lat, double lon)
    {
        var hash = default(GeoCoord).SetDegrees(Convert.ToDecimal(lat), Convert.ToDecimal(lon));

        var h3Location = Api.GeoToH3(hash, GeoMathHelper.ResolutionForCity);
        Api.KRing(h3Location, GeoMathHelper.KRingForResolution, out var neighbours);

        var closestCities = await GetByFilter(c => neighbours
                .Select(n => n.Value)
                .Any(geo => geo == c.GeoHash));

        return closestCities
            .Select(city => new
            {
                city,
                Distance = GeoMathHelper
                    .GetDistanceFromLatLonInKm(
                        city.Latitude,
                        city.Longitude,
                        lat,
                        lon),
            })
            .OrderBy(p => p.Distance)
            .Select(c => c.city)
            .FirstOrDefault();
    }
}