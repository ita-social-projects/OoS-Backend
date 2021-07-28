using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface IElasticsearchSynchronizationService
    {
        Task<bool> Synchronize();
    }
}
