using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IRatingRepository : IEntityRepository<long, Rating>
{
}