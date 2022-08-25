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

    public Task<Tuple<double, int>> GetAverageRatingAsync(Guid entityId, RatingType type)
    {
        return db.Ratings.Where(rating => rating.EntityId == entityId && rating.Type == type)
            .GroupBy(rating => rating.EntityId)
            .Select(g => Tuple.Create(g.Average(e => e.Rate), g.Count()))
            .FirstOrDefaultAsync();
    }

    public Task<Dictionary<Guid, Tuple<double, int>>> GetAverageRatingForEntitiesAsync(IEnumerable<Guid> entityIds, RatingType type)
    {
        return db.Ratings
            .Where(rating => rating.Type == type && entityIds.Contains(rating.EntityId))
            .GroupBy(rating => rating.EntityId)
            .Select(entity => new { EntityId = entity.Key, Average = entity.Average(e => e.Rate), EntityCount = entity.Count() })
            .ToDictionaryAsync(g => g.EntityId, g => Tuple.Create(g.Average, g.EntityCount));
    }
}