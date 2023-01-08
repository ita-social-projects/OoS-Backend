using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;

namespace OutOfSchool.IdentityServer.Services.Interfaces;

public interface IRegionAdminService
{
    Task<ResponseDto> CreateRegionAdminAsync(
        RegionAdminBaseDto regionAdminBaseDto,
        IUrlHelper url,
        string userId,
        string requestId);

    Task<ResponseDto> UpdateRegionAdminAsync(
        RegionAdminBaseDto updateRegionAdminDto,
        string userId,
        string requestId);

    Task<ResponseDto> DeleteRegionAdminAsync(
        string regionAdminId,
        string userId,
        string requestId);

    Task<ResponseDto> BlockRegionAdminAsync(
        string regionAdminId,
        string userId,
        string requestId,
        bool isBlocked);
}