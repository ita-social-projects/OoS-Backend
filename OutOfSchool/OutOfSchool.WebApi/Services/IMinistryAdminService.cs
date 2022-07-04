using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IMinistryAdminService
    {
        Task<ResponseDto> CreateMinistryAdminAsync(string userId, CreateMinistryAdminDto ministryAdminDto, string token);

        Task<ResponseDto> DeleteMinistryAdminAsync(string ministryAdminId, string userId, Guid InstitutionId, string token);

        Task<ResponseDto> BlockMinistryAdminAsync(string ministryAdminId, string userId, Guid InstitutionId, string token);

        Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId);

        Task<IEnumerable<MinistryAdminDto>> GetRelatedMinistryAdmins(string userId);
    }
}
