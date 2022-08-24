using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public interface IProviderAdminService
{
    Task<Either<ErrorResponse, CreateProviderAdminDto>> CreateProviderAdminAsync(
        string userId,
        CreateProviderAdminDto providerAdminDto,
        string token);

    Task<Either<ErrorResponse, ActionResult>> DeleteProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token);

    Task<Either<ErrorResponse, ActionResult>> BlockProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token);

    Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId);

    Task<IEnumerable<ProviderAdminDto>> GetRelatedProviderAdmins(string userId);

    Task<IEnumerable<Guid>> GetRelatedWorkshopIdsForProviderAdmins(string userId);

    Task<bool> CheckUserIsRelatedProviderAdmin(string userId, Guid providerId, Guid workshopId = default);

    Task<IEnumerable<string>> GetProviderAdminsIds(Guid workshopId);

    Task<IEnumerable<string>> GetProviderDeputiesIds(Guid providerId);

    /// <summary>
    /// Get workshops that provider admin can manage.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <param name="isProviderDeputy">Is providerAdmin deputy or no.</param>
    /// <returns>List of the workshops that providerAdmin can mansge.</returns>
    Task<IEnumerable<WorkshopCard>> GetWorkshopsThatProviderAdminCanManage(string userId, bool isProviderDeputy);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderAdminProviderRelationDto> GetById(string userId);
}