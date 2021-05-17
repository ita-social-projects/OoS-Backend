using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IRatingRepository : IEntityRepository<Rating>
    {
        double GetAverageRating(long entityId, RatingType type);

        Dictionary<long, double> GetAverageRatingForEntities(IEnumerable<long> entities, RatingType type);
    }
}
