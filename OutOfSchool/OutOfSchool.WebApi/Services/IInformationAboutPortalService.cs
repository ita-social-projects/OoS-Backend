using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for InformationAboutPortal entity.
    /// </summary>
    public interface IInformationAboutPortalService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="informationAboutPortalDto">InformationAboutPortal entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<InformationAboutPortalDto> Create(InformationAboutPortalDto informationAboutPortalDto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="informationAboutPortalDto">InformationAboutPortal entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<InformationAboutPortalDto> Update(InformationAboutPortalDto informationAboutPortalDto);

        /// <summary>
        /// Get entity.
        /// </summary>
        /// <returns>InformationAboutPortal.</returns>
        Task<InformationAboutPortalDto> GetInformationAboutPortal();
    }
}
