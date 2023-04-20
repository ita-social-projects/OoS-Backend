using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class SensitiveEntityRepository<T> : EntityRepositoryBase<Guid, T>, ISensitiveEntityRepository<T>
    where T : class, IKeyedEntity<Guid>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveEntityRepository{T}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public SensitiveEntityRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
