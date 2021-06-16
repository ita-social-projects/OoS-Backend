using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepository<Workshop>
    {
        /// <summary>
        /// Checks entity subcategoryId existence.
        /// </summary>
        /// <param name="id">Subcategory id.</param>
        /// <returns>True if Subsubcaategory exists, otherwise false.</returns>
        bool SubsubcategoryExists(long id);
    }
}
