using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderRepository : IEntityRepository<Provider>
    {
        bool IsUnique(Provider entity);
    }
}
