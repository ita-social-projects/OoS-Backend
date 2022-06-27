using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class EntityRepository<TKey, TEntity> : EntityRepositoryBase<TKey, TEntity>, IEntityRepository<TKey, TEntity>
    where TEntity : class, IKeyedEntity<TKey>, new()
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepository{TKey, TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public EntityRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
