using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository;

public class ContactsForwardRepository : EntityRepositorySoftDeleted<Guid, Child>, IEntityRepository<Guid, Child>
{
    private readonly OutOfSchoolDbContext db;

    public ContactsForwardRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }

    /// <inheritdoc/>
    public new async Task Delete(ContactsForward entity)
    {
        db.Entry(entity).State = EntityState.Deleted;

        if (entity.Address != null)
        {
            db.Entry(entity.Address).State = EntityState.Deleted;
        }

        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContactsForward>> GetByIds(IEnumerable<Guid> ids)
    {
        return await db.ContactsForwards.Include(contact => contact.Contact).Include(contact => contact.Address)
            .ToListAsync();
    }

    public async Task<ContactsForward> Create(ContactsForward contactsForward)
    {
        await dbContext.AddAsync(contactsForward).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return await Task.FromResult(contactsForward).ConfigureAwait(false);
    }

    public async Task<bool> Update(ContactsForward contactsForward)
    {
        var contactTemp = dbContext.ContactsForwards.FirstOrDefault(contact => contact.Id == contactsForward.Id);
        if (contactTemp is null)
        {
            return false;
        }

        dbContext.ContactsForwards.Update(contactsForward);
        dbContext.SaveChangesAsync();
        return true;
    }
}