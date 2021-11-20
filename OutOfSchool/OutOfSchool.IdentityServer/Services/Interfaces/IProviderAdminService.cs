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
            ProviderAdminDto providerAdminDto,
            HttpRequest request,
            IUrlHelper url,
            string path,
            string userId);

        Task<ResponseDto> DeleteProviderAdminAsync(
            DeleteProviderAdminDto deleteProviderAdminDto,
            HttpRequest request,
            string path,
            string userId);
    }
}
