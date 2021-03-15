using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderRepository : IEntityRepository<Provider>
    {
        bool Exists(Provider entity);
    }
}
