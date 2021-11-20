using System;
using System.Threading.Tasks;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IProviderAdminService
    {
        Task<ResponseDto> CreateProviderAdminAsync(string userId, ProviderAdminDto providerAdmin, string token);

        Task<ResponseDto> DeleteProviderAdminAsync(string providerAdminId, string userId, Guid providerAdmin, string token);
    }
}
