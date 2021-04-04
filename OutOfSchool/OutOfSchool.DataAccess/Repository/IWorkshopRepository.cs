using System.Collections;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.ViewModels;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepository<Workshop>
    {
        Task<IEnumerable> Search(SearchViewModel searchModel);
    }
}