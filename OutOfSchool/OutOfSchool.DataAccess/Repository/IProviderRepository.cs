using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IProviderRepository : IEntityRepository<Provider>, IExistable<Provider>
    {
        bool ExistsUserId(string id);
    }
}
