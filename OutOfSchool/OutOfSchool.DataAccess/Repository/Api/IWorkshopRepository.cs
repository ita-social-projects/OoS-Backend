using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IWorkshopRepository : IEntityRepositorySoftDeleted<Guid, Workshop>
{
    /// <summary>
    /// Retrieves a workshop entity by its Id, including related navigation properties.
    /// </summary>
    /// <param name="id">The unique identifier of the workshop.</param>
    /// <param name="asNoTracking">If true, the entity is retrieved without tracking changes.</param>
    /// <returns>A <see cref="Task{Workshop}"/> representing the result of the asynchronous operation.
    /// The task result contains the workshop entity with its navigation properties loaded, or null if not found.</returns>
    Task<Workshop> GetWithNavigations(Guid id, bool asNoTracking = false);

    Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids);

    /// <summary>
    /// Update ProviderTitle property in all workshops with specified provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerTitle">Full Title of Provider to be changed.</param>
    /// <param name="providerTitleEn">Full English Title of Provider to be changed.</param>
    /// <returns>List of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn);

    /// <summary>
    /// Update IsBlocked property in all workshops with specified provider.
    /// </summary>
    /// <param name="provider">Provider to be searched by.</param>
    /// <returns>List of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> BlockByProvider(Provider provider);

    /// <summary>
    /// Return amount of available seats for specified workshop.
    /// </summary>
    /// <param name="workshopId">Id of Workshop.</param>
    /// <returns>Amount of available seats for the specified workshop.</returns>
    /// <exception cref="InvalidOperationException">It can throw exception when method get workshopId but Workshop doesn't exist.</exception>
    Task<uint> GetAvailableSeats(Guid workshopId);

    Task<List<Workshop>> GetAllWithDeleted(DateTime updatedAfter, int from, int size);
    
    Task<int> CountWithDeleted(DateTime updatedAfter);
}
