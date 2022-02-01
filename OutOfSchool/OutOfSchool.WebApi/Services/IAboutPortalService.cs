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
        /// Update entity.
        /// </summary>
        /// <param name="dto">AboutPortal entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<AboutPortalDto> Update(AboutPortalDto dto);

        /// <summary>
        /// Get entity.
        /// </summary>
        /// <returns>InformationAboutPortal.</returns>
        Task<AboutPortalDto> Get();
    }
}
