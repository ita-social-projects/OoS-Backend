using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IRegionAdminRepository : IEntityRepository<(string, long), RegionAdmin>
{
    Task<RegionAdmin> GetByIdAsync(string userId);
}