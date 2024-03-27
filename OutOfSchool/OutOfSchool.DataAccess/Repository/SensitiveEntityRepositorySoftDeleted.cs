using System;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class SensitiveEntityRepositorySoftDeleted<T> : EntityRepositorySoftDeleted<Guid, T>, ISensitiveEntityRepositorySoftDeleted<T>
    where T : class, IKeyedEntity<Guid>, ISoftDeleted, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SensitiveEntityRepositorySoftDeleted{T}"/> class.
    /// </summary>
    /// <param name="dbContext">OutOfSchoolDbContext.</param>
    public SensitiveEntityRepositorySoftDeleted(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
