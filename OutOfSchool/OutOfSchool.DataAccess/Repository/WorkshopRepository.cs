using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.ViewModels;

namespace OutOfSchool.Services.Repository
{
    public class WorkshopRepository : EntityRepository<Workshop>, IWorkshopRepository
    {
        private readonly OutOfSchoolDbContext context;
        
        public WorkshopRepository(OutOfSchoolDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable> Search(SearchViewModel searchModel)
        {
            IEnumerable workshops = null;
            
            if (!string.IsNullOrEmpty(searchModel.Title.ToLower()))
            {
                workshops = await GetByCondition(w => w.Title.Contains(searchModel.Title.ToLower())).ConfigureAwait(false);
            }

            return workshops;
        }
    }
}