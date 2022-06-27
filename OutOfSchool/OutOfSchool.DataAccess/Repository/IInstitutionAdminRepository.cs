﻿using System;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository
{
    public interface IInstitutionAdminRepository : IEntityRepository<InstitutionAdmin>
    {
        Task<bool> IsExistInstitutionAdminDeputyWithUserIdAsync(Guid InstitutionId, string userId);

        Task<bool> IsExistInstitutionWithUserIdAsync(string userId);

        Task<Institution> GetInstitutionWithUserIdAsync(string userId);

        Task AddRelatedWorkshopForAssistant(string userId, Guid workshopId);

        Task<InstitutionAdmin> GetByIdAsync(string userId, Guid InstitutionId);
    }
}