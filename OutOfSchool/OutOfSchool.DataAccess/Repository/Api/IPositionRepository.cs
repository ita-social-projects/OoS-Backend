using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository.Api;
public interface IPositionRepository
{
    Task<Position> CreateAsync(Position entity);
    Task<Position> GetByIdAsync(Guid id);
    Task<IEnumerable<Position>> GetAllAsync(Guid id);
    Task<Position> UpdateAsync(Position entity);
    Task DeleteAsync(Position entity); 
}
