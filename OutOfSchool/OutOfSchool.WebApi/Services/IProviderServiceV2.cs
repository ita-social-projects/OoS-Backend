using System;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Services;

public interface IProviderServiceV2 : IProviderService
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ProviderDto"/>.</returns>
    new Task<ProviderDto> Create(ProviderDto dto);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <param name="userId">User id.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ProviderDto"/>.</returns>
    new Task<ProviderDto> Update(ProviderUpdateDto dto, string userId);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    new Task Delete(Guid id);
}