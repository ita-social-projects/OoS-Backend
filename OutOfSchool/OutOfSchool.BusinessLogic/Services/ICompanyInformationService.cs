using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for the CompanyInformation entity.
/// </summary>
public interface ICompanyInformationService
{
    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="companyInformationDto">CompanyInformation entity to update.</param>
    /// <param name="type">CompanyInformationType that will be updated.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<CompanyInformationDto> Update(CompanyInformationDto companyInformationDto, CompanyInformationType type);

    /// <summary>
    /// Get CompanyInformation entity by it's type.
    /// </summary>
    /// <param name="type">Type in the table.</param>
    /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<CompanyInformationDto> GetByType(CompanyInformationType type);
}