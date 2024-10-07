using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface IEmployeeService
{
    Task<ResponseDto> CreateEmployeeAsync(
        CreateEmployeeDto employeeDto,
        IUrlHelper url,
        string userId);

    Task<ResponseDto> UpdateEmployeeAsync(
        UpdateEmployeeDto employeeUpdateDto,
        string userId);

    Task<ResponseDto> DeleteEmployeeAsync(
        string employeeId,
        string userId);

    Task<ResponseDto> BlockEmployeeAsync(
        string employeeId,
        string userId,
        bool isBlocked);

    Task<ResponseDto> BlockEmployeesByProviderAsync(
        Guid providerId,
        string userId,
        bool isBlocked);

    Task<ResponseDto> ReinviteEmployeeAsync(
        string employeeId,
        string userId,
        IUrlHelper url);
}