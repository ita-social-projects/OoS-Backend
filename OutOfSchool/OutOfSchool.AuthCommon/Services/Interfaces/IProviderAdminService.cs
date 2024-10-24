using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface IProviderAdminService
{
    Task<ResponseDto> CreateProviderAdminAsync(
        CreateProviderAdminDto providerAdminDto,
        IUrlHelper url,
        string userId);

    Task<ResponseDto> UpdateProviderAdminAsync(
        UpdateProviderAdminDto providerAdminUpdateDto,
        string userId);

    Task<ResponseDto> DeleteProviderAdminAsync(
        string providerAdminId,
        string userId);

    Task<ResponseDto> BlockProviderAdminAsync(
        string providerAdminId,
        string userId,
        bool isBlocked);

    Task<ResponseDto> BlockProviderAdminsAndDeputiesByProviderAsync(
        Guid providerId,
        string userId,
        bool isBlocked);

    Task<ResponseDto> ReinviteProviderAdminAsync(
        string providerAdminId,
        string userId,
        IUrlHelper url);
}