using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IQuartzJobRepository : IEntityRepository<long, QuartzJob>
{
}