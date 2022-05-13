using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface ICompanyInformationRepository : ISensitiveEntityRepository<CompanyInformation>
    {
        Task<CompanyInformation> GetWithNavigationsByTypeAsync(CompanyInformationType type);

        void DeleteAllItemsByEntityAsync(CompanyInformation entity);

        Task CreateItems(IEnumerable<CompanyInformationItem> entities);
    }
}
