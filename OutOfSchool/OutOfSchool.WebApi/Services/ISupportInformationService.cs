using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for SupportInformation entity.
    /// </summary>
    public interface ISupportInformationService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="supportInformationDto">SupportInformation entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SupportInformationDto> Create(SupportInformationDto supportInformationDto);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="supportInformationDto">SupportInformation entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SupportInformationDto> Update(SupportInformationDto supportInformationDto);

        /// <summary>
        /// Get entity.
        /// </summary>
        /// <returns>SupportInformation.</returns>
        Task<SupportInformationDto> GetSupportInformation();
    }
}
