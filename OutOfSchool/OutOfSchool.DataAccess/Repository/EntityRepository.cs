using System;

namespace OutOfSchool.Services.Repository;

public class EntityRepository<T> : EntityRepositoryBase<long, T>, IEntityRepository<T>
    where T : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public EntityRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}