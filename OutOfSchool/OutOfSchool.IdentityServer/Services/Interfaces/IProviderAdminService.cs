using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.IdentityServer.Services.Intefaces
{
    public interface IProviderAdminService
    {
        Task<ResponseDto> CreateProviderAdminAsync(
            CreateProviderAdminDto providerAdminDto,
            IUrlHelper url,
            string path,
            string userId,
            string requestId);

        Task<ResponseDto> DeleteProviderAdminAsync(
            string providerAdminId,
            string path,
            string userId,
            string requestId);

        Task<ResponseDto> BlockProviderAdminAsync(
            string providerAdminId,
            string path,
            string userId,
            string requestId);
    }
}
