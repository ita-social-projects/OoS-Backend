using System;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;
public class EmailOutboxRepository : EntityRepository<Guid, EmailOutbox>, IEmailOutboxRepository
{
    public EmailOutboxRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}
