using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IRatingRepository : IEntityRepository<Rating>
    {
        Tuple<double, int> GetAverageRating(long entityId, RatingType type);

        Dictionary<long, Tuple<double, int>> GetAverageRatingForEntities(IEnumerable<long> entityIds, RatingType type);
    }
}
