using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IPositionRepository : ISensitiveEntityRepositorySoftDeleted<Position>
{
    Task<bool> DirectorExistsAsync(Guid providerId);
}