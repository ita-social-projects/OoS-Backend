using System;

namespace OutOfSchool.Services.Repository;

public class SensitiveEntityRepository<T> : EntityRepositoryBase<Guid, T>, ISensitiveEntityRepository<T>
    where T : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public SensitiveEntityRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}