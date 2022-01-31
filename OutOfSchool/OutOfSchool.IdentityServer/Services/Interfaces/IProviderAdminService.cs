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
            HttpRequest request,
            IUrlHelper url,
            string path,
            string userId);

        Task<ResponseDto> DeleteProviderAdminAsync(
            string providerAdminId,
            HttpRequest request,
            string path,
            string userId);

        Task<ResponseDto> BlockProviderAdminAsync(
            string providerAdminId,
            HttpRequest request,
            string path,
            string userId);
    }
}
