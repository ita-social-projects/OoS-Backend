using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface ISubcategoryRepository : IEntityRepository<Subcategory>
    {
        bool SameExists(Subcategory entity);

        bool CategoryExists(long id);
    }
}
