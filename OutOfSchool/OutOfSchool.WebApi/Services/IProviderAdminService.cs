using System;
using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IProviderAdminService
    {
        Task<ResponseDto> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdmin, string token);

        Task<ResponseDto> DeleteProviderAdminAsync(string providerAdminId, string userId, Guid providerAdmin, string token);

        Task<ResponseDto> BlockProviderAdminAsync(string providerAdminId, string userId, Guid providerAdmin, string token);

        Task<bool> CheckUserIsRelatedProviderAdmin(string providerAdminId, Guid providerId, Guid workshopId = default);
    }
}
