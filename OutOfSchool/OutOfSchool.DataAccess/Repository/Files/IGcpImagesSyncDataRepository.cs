using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository.Files
{
    public interface IGcpImagesSyncDataRepository
    {
        Task<List<string>> GetIntersectWorkshopCoverImagesIds(IEnumerable<string> searchIds);

        Task<List<string>> GetIntersectTeacherCoverImagesIds(IEnumerable<string> searchIds);

        Task<List<string>> GetIntersectProviderCoverImagesIds(IEnumerable<string> searchIds);

        Task<List<string>> GetIntersectWorkshopImagesIds(IEnumerable<string> searchIds);

        Task<List<string>> GetIntersectProviderImagesIds(IEnumerable<string> searchIds);
    }
}