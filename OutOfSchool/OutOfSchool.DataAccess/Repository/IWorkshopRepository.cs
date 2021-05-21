using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepository<Workshop>
    {
        new Task<Workshop> Create(Workshop entity);

        bool SubsubcategoryExists(long id);
    }
}
