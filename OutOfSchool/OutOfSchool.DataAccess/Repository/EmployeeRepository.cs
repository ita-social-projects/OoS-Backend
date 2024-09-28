using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class EmployeeRepository : EntityRepositorySoftDeleted<(string, Guid), Employee>, IEmployeeRepository
{
    private readonly OutOfSchoolDbContext db;

    public EmployeeRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public async Task<Employee?> GetByIdAsync(string userId, Guid providerId)
    {
        return await db.Employees
            .Where(pa => pa.ProviderId == providerId && !pa.IsDeleted)
            .SingleOrDefaultAsync(pa => pa.UserId == userId);
    }

    public async Task<bool> IsExistEmployeeWithUserIdAsync(Guid providerId, string userId)
    {
        var employee = await GetByIdAsync(userId, providerId);

        return employee != null;
    }

    public async Task<bool> IsExistProviderWithUserIdAsync(string userId)
    {
        var provider = await db.Providers
            .SingleOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        return provider != null;
    }

    public async Task<Provider> GetProviderWithUserIdAsync(string userId)
    {
        var provider = await db.Providers
            .SingleOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        return provider;
    }

    public async Task AddRelatedWorkshopForEmployee(string userId, Guid workshopId)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(p => p.UserId == userId);
        var workshopToUpdate = await db.Workshops.SingleOrDefaultAsync(w => w.Id == workshopId);
        workshopToUpdate.Employees = new List<Employee> { employee };
        db.Update(workshopToUpdate);
        await db.SaveChangesAsync();
    }

    public async Task<int> GetNumberEmployeesAsync(Guid providerId)
    {
        return await db.Employees
            .CountAsync(pa => pa.ProviderId == providerId && !pa.IsDeleted);
    }
}