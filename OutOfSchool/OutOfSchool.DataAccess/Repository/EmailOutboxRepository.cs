using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;
public class EmailOutboxRepository : EntityRepository<long, EmailOutbox>, IEmailOutboxRepository
{
    public EmailOutboxRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext) { }
}
