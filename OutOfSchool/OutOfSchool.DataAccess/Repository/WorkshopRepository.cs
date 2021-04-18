using System.Collections;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

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

        public async Task<IEnumerable> Search(string searchString)
        {
            IEnumerable workshops = null;
            
            if (!string.IsNullOrEmpty(searchString.ToLower()))
            {
                workshops = await GetByCondition(w => w.Title.Contains(searchString.ToLower())).ConfigureAwait(false);
            }

            return workshops;
        }
    }
}