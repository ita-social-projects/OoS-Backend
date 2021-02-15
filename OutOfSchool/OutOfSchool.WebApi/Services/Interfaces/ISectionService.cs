using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    /// <summary>
    /// Interface of SectionService.
    /// </summary>
    public interface ISectionService
    {
        /// <summary>
        /// Add a new Section to the database.
        /// </summary>
        /// <param name="section">Entity which needs to be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SectionDTO> Create(SectionDTO section);
        
        /// <summary>
        /// Get all sections from the database.
        /// </summary>
        /// <returns>List of all sections.</returns>
        IEnumerable<SectionDTO> GetAllSections();
    }
}