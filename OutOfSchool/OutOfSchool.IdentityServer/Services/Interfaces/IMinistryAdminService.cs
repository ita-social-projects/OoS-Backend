using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;

namespace OutOfSchool.IdentityServer.Services.Interfaces;

public interface IMinistryAdminService
{
    Task<ResponseDto> CreateMinistryAdminAsync(
        MinistryAdminBaseDto ministryAdminBaseDto,
        IUrlHelper url,
        string userId,
        string requestId);

    Task<ResponseDto> UpdateMinistryAdminAsync(
        MinistryAdminBaseDto updateMinistryAdminDto,
        string userId,
        string requestId);

    Task<ResponseDto> DeleteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string requestId);

    Task<ResponseDto> BlockMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string requestId,
        bool isBlocked);
}