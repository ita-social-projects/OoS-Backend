﻿using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository
{
    public interface IMinistryAdminRepository : IEntityRepository<MinistryAdmin>
    {
        Task<Institution> GetInstitutionWithUserIdAsync(string userId);

        Task AddRelatedWorkshopForAssistant(string userId, Guid workshopId);
        
        Task<MinistryAdmin> GetByIdAsync(string userId);
    }
}