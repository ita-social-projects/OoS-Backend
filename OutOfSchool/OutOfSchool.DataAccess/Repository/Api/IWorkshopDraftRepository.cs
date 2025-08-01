using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;
public interface IWorkshopDraftRepository : IEntityRepository<Guid, WorkshopDraft>
{
    /// <summary>
    /// Asynchronously retrieves all workshop drafts for a specific provider.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains enumerable collection of workshop drafts associated with the specified provider.
    /// Returns an empty collection if no drafts are found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="providerId"/> is <c>null</c>.</exception>
    Task<IEnumerable<WorkshopDraft>> GetByProviderIdAsync(Guid providerId);

    /// <summary>
    /// Update ProviderTitle and ProviderTitleEn in all workshop drafts for a provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerTitle">Full Title of Provider to be changed.</param>
    /// <param name="providerTitleEn">Full English Title of Provider to be changed.</param>
    /// <returns>List of WorkshopDrafts for the specified provider.</returns>
    Task<IEnumerable<WorkshopDraft>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn);
}
