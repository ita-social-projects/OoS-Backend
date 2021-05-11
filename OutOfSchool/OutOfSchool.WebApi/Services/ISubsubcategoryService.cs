using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Category entity.
    /// </summary>
    public interface ISubsubcategoryService
    {
        /// <summary>
        /// Add new Category to the DB.
        /// </summary>
        /// <param name="dto">CategoryDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SubsubcategoryDTO> Create(SubsubcategoryDTO dto);

        /// <summary>
        /// Get all Category objects from DB.
        /// </summary>
        /// <returns>List of Category objects.</returns>
        Task<IEnumerable<SubsubcategoryDTO>> GetAll();

        /// <summary>
        /// To recieve the Category object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Category object.</returns>
        Task<SubsubcategoryDTO> GetById(long id);

        /// <summary>
        /// To recieve the Subsubcategory list with defined SubcategoryId.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of subsubcategories.</returns>
        Task<IEnumerable<SubsubcategoryDTO>> GetBySubcategoryId(long id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="dto">Category with new properties.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<SubsubcategoryDTO> Update(SubsubcategoryDTO dto);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
