using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Gcp
{
    public interface IGcpStorageSynchronizationService
    {
        Task SynchronizeAsync(CancellationToken cancellationToken = default);
    }
}