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
    public interface ISubcategoryService
    {
        /// <summary>
        /// Add new Category to the DB.
        /// </summary>
        /// <param name="category">CategoryDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SubcategoryDTO> Create(SubcategoryDTO category);

        /// <summary>
        /// Get all Category objects from DB.
        /// </summary>
        /// <returns>List of Category objects.</returns>
        Task<IEnumerable<SubcategoryDTO>> GetAll();

        /// <summary>
        /// To recieve the Category object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>Category object.</returns>
        Task<SubcategoryDTO> GetById(long id);

        /// <summary>
        /// To recieve the Subcategory list with define Categoryid.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>List of subcategories.</returns>
        Task<IEnumerable<SubcategoryDTO>> GetByCategoryId(long id);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="category">Category with new properties.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<SubcategoryDTO> Update(SubcategoryDTO category);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}
