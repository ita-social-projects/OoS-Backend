using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IRatingRepository : IEntityRepository<long, Rating>
{
    Tuple<double, int> GetAverageRating(Guid entityId, RatingType type);

    Dictionary<Guid, Tuple<double, int>> GetAverageRatingForEntities(IEnumerable<Guid> entityIds, RatingType type);
}