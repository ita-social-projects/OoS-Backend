using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public interface IStatusService
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all InstitutionStatuses.</returns>
    Task<IEnumerable<InstitutionStatusDTO>> GetAll(LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>InstitutionStatus.</returns>
    Task<InstitutionStatusDTO> GetById(long id, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">InstitutionStatus entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<InstitutionStatusDTO> Create(InstitutionStatusDTO dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">InstitutionStatus entity to add.</param>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<InstitutionStatusDTO> Update(InstitutionStatusDTO dto, LocalizationType localization = LocalizationType.Ua);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">InstitutionStatus key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}