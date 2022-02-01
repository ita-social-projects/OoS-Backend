using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IAboutPortalRepository : ISensitiveEntityRepository<AboutPortal>
    {
        IUnitOfWork UnitOfWork { get; }

        Task<AboutPortal> GetWithNavigations();

        void DeleteAllItems();

        Task CreateItems(IEnumerable<AboutPortalItem> entities);
    }
}
