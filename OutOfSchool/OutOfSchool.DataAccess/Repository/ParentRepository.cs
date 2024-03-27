using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ParentRepository : EntityRepositorySoftDeleted<Guid, Parent>, IParentRepository
{
    private readonly OutOfSchoolDbContext db;

    public ParentRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <summary>
    /// Add new element.
    /// </summary>
    /// <param name="entity">Entity to create.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public new async Task<Parent> Create(Parent entity)
    {
        await db.Parents.AddAsync(entity);
        await db.SaveChangesAsync();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == entity.UserId).ConfigureAwait(false);
        user.IsRegistered = true;
        db.Entry(user).State = EntityState.Modified;
        await db.SaveChangesAsync();

        var child = new Child()
        {
            Id = Guid.Empty,
            IsParent = true,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName ?? string.Empty,
            LastName = user.LastName,
            Gender = entity.Gender,
            DateOfBirth = entity.DateOfBirth,
            ParentId = entity.Id,
        };
        await db.Children.AddAsync(child);
        await db.SaveChangesAsync();

        return await Task.FromResult(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Parent>> GetByIdsAsync(IEnumerable<Guid> parentIds)
    {
        _ = parentIds ?? throw new ArgumentNullException(nameof(parentIds));

        var parents = await db.Parents
            .Where(parent => parentIds.Contains(parent.Id))
            .ToListAsync();

        return parents;
    }
}