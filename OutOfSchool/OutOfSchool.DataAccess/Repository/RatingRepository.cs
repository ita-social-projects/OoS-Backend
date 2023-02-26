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
    public RatingRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}