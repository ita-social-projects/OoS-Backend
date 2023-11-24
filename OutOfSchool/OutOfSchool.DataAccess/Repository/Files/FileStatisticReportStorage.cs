using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files;

public class FileStatisticReportStorage : FileInDbStorageBase<FileModel>, IStatisticReportFileStorage
{
    public FileStatisticReportStorage(IFileInDbRepository fileInDbRepository)
        : base(fileInDbRepository)
    {
    }
}