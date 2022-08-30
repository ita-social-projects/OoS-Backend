using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IRatingRepository : IEntityRepository<long, Rating>
{
    Task<Tuple<double, int>> GetAverageRatingAsync(Guid entityId, RatingType type);

    Task<Dictionary<Guid, Tuple<double, int>>> GetAverageRatingForEntitiesAsync(IEnumerable<Guid> entityIds, RatingType type);
}