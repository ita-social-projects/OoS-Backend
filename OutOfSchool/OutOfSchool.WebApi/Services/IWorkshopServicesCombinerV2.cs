using System;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services;

public interface IWorkshopServicesCombinerV2 : IWorkshopServicesCombiner
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopCreationResultDto"/>.</returns>
    new Task<WorkshopCreationResultDto> Create(WorkshopDTO dto);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopUpdateResultDto"/>.</returns>
    new Task<WorkshopUpdateResultDto> Update(WorkshopDTO dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    new Task Delete(Guid id);
}