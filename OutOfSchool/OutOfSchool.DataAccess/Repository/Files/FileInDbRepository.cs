using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files;

public class FileInDbRepository : EntityRepositoryBase<string, FileInDb>, IFileInDbRepository
{
    private readonly OutOfSchoolDbContext db;

    public FileInDbRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
        db = dbContext;
    }
}