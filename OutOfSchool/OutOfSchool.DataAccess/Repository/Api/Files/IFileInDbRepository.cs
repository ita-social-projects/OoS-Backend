using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api.Files;

public interface IFileInDbRepository : IEntityRepositoryBase<string, FileInDb>
{
}