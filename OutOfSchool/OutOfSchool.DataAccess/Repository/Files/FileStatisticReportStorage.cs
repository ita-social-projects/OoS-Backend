using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files;

public class FileStatisticReportStorage : FileInDbStorageBase<FileModel>, IStatisticReportFileStorage
{
    public FileStatisticReportStorage(IFileInDbRepository fileInDbRepository)
        : base(fileInDbRepository)
    {
    }
}