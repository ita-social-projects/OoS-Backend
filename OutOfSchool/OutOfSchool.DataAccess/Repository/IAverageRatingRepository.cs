using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository;

public interface IAverageRatingRepository : IEntityRepository<long, AverageRating>
{
}
