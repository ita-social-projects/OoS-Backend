using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;
public interface IEmailOutboxRepository : IEntityRepository<long, EmailOutbox>
{
}
