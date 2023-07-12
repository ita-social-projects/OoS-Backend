using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface ICommonMinistryAdminService<TDto>
    where TDto : MinistryAdminBaseDto
{
    Task<ResponseDto> CreateMinistryAdminAsync(
        TDto ministryAdminBaseDto,
        Role role,
        IUrlHelper url,
        string userId,
        string requestId);

    Task<ResponseDto> UpdateMinistryAdminAsync(
        TDto updateMinistryAdminDto,
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

    Task<ResponseDto> ReinviteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        IUrlHelper url,
        string requestId);
}