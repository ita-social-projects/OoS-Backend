﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class RatingRepository : EntityRepository<Rating>, IRatingRepository
    {
        private readonly OutOfSchoolDbContext db;

        public RatingRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        public double GetAverageRating(long entityId, RatingType type)
        {
            var ratings = db.Ratings.Where(rating => rating.EntityId == entityId && rating.Type == type);

            if (ratings.Count() != 0)
            {
                return ratings.Average(rating => rating.Rate);
            }
            else
            {
                return default;
            }
        }

        public Dictionary<long, double> GetAverageRatingForEntities(IEnumerable<long> entities, RatingType type)
        {
            return db.Ratings
                .Where(r => r.Type == type)
                .AsEnumerable()
                .GroupBy(g => g.EntityId)
                .ToDictionary(g => g.Key, g => g.Average(p => p.Rate));
        }
    }
}
