using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services;

public interface IEmployeeService
{
    Task<Either<ErrorResponse, CreateEmployeeDto>> CreateEmployeeAsync(
        string userId,
        CreateEmployeeDto employeeDto,
        string token);

    Task<Either<ErrorResponse, UpdateEmployeeDto>> UpdateEmployeeAsync(
        UpdateEmployeeDto employeeModel,
        string userId,
        Guid providerId,
        string token);

    Task<Either<ErrorResponse, ActionResult>> DeleteEmployeeAsync(
        string employeeId,
        string userId,
        Guid providerId,
        string token);

    Task<Either<ErrorResponse, ActionResult>> BlockEmployeeAsync(
        string employeeId,
        string userId,
        Guid providerId,
        string token,
        bool isBlocked);

    Task<Either<ErrorResponse, ActionResult>> BlockEmployeeByProviderAsync(
        Guid providerId,
        string userId,
        string token,
        bool isBlocked);

    Task<IEnumerable<EmployeeDto>> GetRelatedEmployees(string userId);

    Task<IEnumerable<Guid>> GetRelatedWorkshopIdsForEmployees(string userId);

    Task<bool> CheckUserIsRelatedEmployee(string userId, Guid providerId, Guid workshopId = default);

    Task<IEnumerable<string>> GetEmployeesIds(Guid workshopId);

    /// <summary>
    /// Get workshops that employee can manage.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <returns>List of the workshops that employee can manage.</returns>
    Task<SearchResult<WorkshopProviderViewCard>> GetWorkshopsThatEmployeeCanManage(string userId);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<EmployeeProviderRelationDto> GetById(string userId);

    /// <summary>
    /// Get Employee by it's Id.
    /// </summary>
    /// <param name="employeeId">Employee's Id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<FullEmployeeDto> GetFullEmployee(string employeeId);

    Task GiveEmployeeAccessToWorkshop(string userId, Guid workshopId);

    /// <summary>
    /// Send invitation to Employee by Id.
    /// </summary>
    /// <param name="employeeId">Employees's Id.</param>
    /// <param name="userId">Current user's Id.</param>
    /// <param name="token">Current user's token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteEmployeeAsync(
    string employeeId,
    string userId,
    string token);
}
