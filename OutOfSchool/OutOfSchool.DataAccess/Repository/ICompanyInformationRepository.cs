using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface ICompanyInformationRepository : ISensitiveEntityRepository<AboutPortal>
    {
        Task<AboutPortal> GetWithNavigationsByTypeAsync(CompanyInformationType type);

        void DeleteAllItemsByEntityAsync(AboutPortal entity);

        Task CreateItems(IEnumerable<AboutPortalItem> entities);
    }
}
