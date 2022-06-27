using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IInstitutionAdminService
    {
        Task<ResponseDto> CreateInstitutionAdminAsync(string userId, CreateInstitutionAdminDto InstitutionAdminDto, string token);

        Task<ResponseDto> DeleteInstitutionAdminAsync(string InstitutionAdminId, string userId, Guid InstitutionId, string token);

        Task<ResponseDto> BlockInstitutionAdminAsync(string InstitutionAdminId, string userId, Guid InstitutionId, string token);

        Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId);

        Task<IEnumerable<InstitutionAdminDto>> GetRelatedInstitutionAdmins(string userId);
    }
}
