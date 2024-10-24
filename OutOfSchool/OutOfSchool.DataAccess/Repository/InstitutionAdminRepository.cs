﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository;

public class InstitutionAdminRepository : EntityRepositorySoftDeleted<(string, Guid), InstitutionAdmin>, IInstitutionAdminRepository
{
    private readonly OutOfSchoolDbContext db;

    public InstitutionAdminRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    public async Task<InstitutionAdmin> GetByIdAsync(string userId)
    {
        return await db.InstitutionAdmins
            .SingleOrDefaultAsync(pa => pa.UserId == userId);
    }
}