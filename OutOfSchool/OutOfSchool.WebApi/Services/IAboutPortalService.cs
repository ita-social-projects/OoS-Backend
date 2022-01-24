using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for InformationAboutPortal entity.
    /// </summary>
    public interface IAboutPortalService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="informationAboutPortalDto">InformationAboutPortal entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AboutPortalDto> Create(AboutPortalDto informationAboutPortalDto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="informationAboutPortalDto">InformationAboutPortal entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AboutPortalDto> Update(AboutPortalDto informationAboutPortalDto);

        /// <summary>
        /// Get entity.
        /// </summary>
        /// <returns>InformationAboutPortal.</returns>
        Task<AboutPortalDto> GetInformationAboutPortal();

        Task<AboutPortalItemDto> GetItemById(Guid id);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="informationAboutPortalItemDto">InformationAboutPortalItem entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AboutPortalItemDto> CreateItem(AboutPortalItemDto informationAboutPortalItemDto);

        Task<AboutPortalItemDto> UpdateItem(AboutPortalItemDto informationAboutPortalItemDto);

        Task DeleteItem(Guid id);

        Task<IEnumerable<AboutPortalItemDto>> GetAllItems();
    }
}
