using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.Services.Repository.Api.Files;

namespace OutOfSchool.Services.Repository.Files;

public class FileStatisticReportStorage : FileInDbStorageBase<FileModel>, IStatisticReportFileStorage
{
    public FileStatisticReportStorage(IFileInDbRepository fileInDbRepository)
        : base(fileInDbRepository)
    {
    }
}