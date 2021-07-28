using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public class ElasticsearchSynchronizationService : IElasticsearchSynchronizationService
    {
        public async Task<bool> Synchronize()
        {
            return true;
        }
    }
}
