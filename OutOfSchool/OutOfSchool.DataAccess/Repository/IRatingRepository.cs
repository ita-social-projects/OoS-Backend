using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IRatingRepository : IEntityRepository<Rating>
    {
        double GetAverageRating(long entityId, RatingType type);

        Dictionary<long, Tuple<double, int>> GetAverageRatingForEntities(IEnumerable<long> entities, RatingType type);
    }
}
