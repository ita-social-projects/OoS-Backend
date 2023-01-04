using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files;

public class FileInDbRepository : EntityRepositoryBase<string, FileInDb>, IFileInDbRepository
{
    public FileInDbRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}