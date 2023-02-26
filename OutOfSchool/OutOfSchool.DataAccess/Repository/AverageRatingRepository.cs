using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository;
public class AverageRatingRepository : EntityRepository<long, AverageRating>, IAverageRatingRepository
{
    public AverageRatingRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
