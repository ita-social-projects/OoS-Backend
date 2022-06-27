using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.IdentityServer.Services.Intefaces
{
    public interface IInstitutionAdminService
    {
        Task<ResponseDto> CreateInstitutionAdminAsync(
            CreateInstitutionAdminDto institutionAdminDto,
            IUrlHelper url,
            string userId,
            string requestId);

        Task<ResponseDto> DeleteInstitutionAdminAsync(
            string institutionAdminId,
            string userId,
            string requestId);

        Task<ResponseDto> BlockInstitutionAdminAsync(
            string institutionAdminId,
            string userId,
            string requestId);
    }
}
