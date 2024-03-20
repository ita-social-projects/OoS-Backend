using System;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;
public interface IEmailOutboxRepository : IEntityRepository<Guid, EmailOutbox>
{
}
