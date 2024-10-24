using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api.Files;
using OutOfSchool.Services.Repository.Base;

namespace OutOfSchool.Services.Repository.Files;

public class FileInDbRepository : EntityRepositoryBase<string, FileInDb>, IFileInDbRepository
{
    public FileInDbRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }
}