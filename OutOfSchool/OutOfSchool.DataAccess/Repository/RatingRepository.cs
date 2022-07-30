using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class RatingRepository : EntityRepository<long, Rating>, IRatingRepository
{
    private readonly OutOfSchoolDbContext db;

    public RatingRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public Tuple<double, int> GetAverageRating(Guid entityId, RatingType type)
    {
        var ratings = db.Ratings.Where(rating => rating.EntityId == entityId && rating.Type == type);

        if (ratings.Count() != 0)
        {
            return new Tuple<double, int>(ratings.Average(rating => rating.Rate), ratings.Count());
        }
        else
        {
            return default;
        }
    }

    public Dictionary<Guid, Tuple<double, int>> GetAverageRatingForEntities(IEnumerable<Guid> entityIds, RatingType type)
    {
        return db.Ratings
            .Where(rating => rating.Type == type && entityIds.Contains(rating.EntityId))
            .AsEnumerable()
            .GroupBy(rating => rating.EntityId)
            .ToDictionary(g => g.Key, g => Tuple.Create(g.Average(p => p.Rate), g.Count()));
    }

    public Task<List<Rating>> GetPartAsync(int skip, int take)
    {
        return db.Ratings.Skip(skip).Take(take).ToListAsync();
    }
}