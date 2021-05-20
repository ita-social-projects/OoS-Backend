using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface ISubsubcategoryRepository : IEntityRepository<Subsubcategory>
    {
        bool SameExists(Subsubcategory entity);

        bool SubcategoryExists(long id);
    }
}
