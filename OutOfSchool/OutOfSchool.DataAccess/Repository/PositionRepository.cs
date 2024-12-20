using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace OutOfSchool.Services.Repository;
public class PositionRepository : IPositionRepository
{
    private readonly OutOfSchoolDbContext dbContext;
    private readonly DbSet<Position> dbSet;
    
    public PositionRepository(OutOfSchoolDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.dbSet = dbContext.Set<Position>();
    }

    public async Task<Position> CreateAsync(Position entity)
    {
        await dbSet.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<Position> GetByIdAsync(Guid id)
    {
        return await dbSet.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Position>> GetAllAsync(Guid id)
    {
        return await dbSet.Where(x => x.CreatedBy == id.ToString()).ToListAsync();
    }

    public async Task<Position> UpdateAsync(Position entity)
    {
        dbSet.Update(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Position entity)
    {
        dbSet.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Position>> GetByProviderIdAsync(Guid providerId)
    {
        return await dbSet.Where(p => p.ProviderId == providerId).ToListAsync();
    }   
}
