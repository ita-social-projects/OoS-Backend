using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

public interface IInstitutionAdminRepositoryBase<TId, TEntity> : IEntityRepositorySoftDeleted<(string, TId), TEntity>
    where TEntity : InstitutionAdminBase, IKeyedEntity<(string, TId)>, new()
{
    Task<TEntity> GetByIdAsync(string userId);
}