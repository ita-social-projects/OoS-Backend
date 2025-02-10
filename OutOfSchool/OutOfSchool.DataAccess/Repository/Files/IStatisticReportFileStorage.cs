using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.Services.Repository.Files;

public interface IStatisticReportFileStorage : IFilesStorage<FileModel, string>
{
}
