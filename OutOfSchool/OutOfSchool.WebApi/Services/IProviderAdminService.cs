using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IProviderAdminService
    {
        Task<ResponseDto> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdmin, string token);

        Task<ResponseDto> DeleteProviderAdminAsync(string providerAdminId, string userId, Guid providerId, string token);

        Task<ResponseDto> BlockProviderAdminAsync(string providerAdminId, string userId, Guid providerId, string token);

        Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId);

        Task<IEnumerable<ProviderAdminDto>> GetRelatedProviderAdmins(string userId);

        Task<bool> CheckUserIsRelatedProviderAdmin(string providerAdminId, Guid providerId, Guid workshopId = default);
    }
}
