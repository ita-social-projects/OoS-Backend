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
        string userId);

    Task<ResponseDto> UpdateMinistryAdminAsync(
        TDto updateMinistryAdminDto,
        string userId);

    Task<ResponseDto> DeleteMinistryAdminAsync(
        string ministryAdminId,
        string userId);

    Task<ResponseDto> BlockMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        bool isBlocked);

    Task<ResponseDto> ReinviteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        IUrlHelper url);
}