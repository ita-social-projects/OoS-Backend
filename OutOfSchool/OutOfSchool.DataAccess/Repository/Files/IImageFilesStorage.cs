using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files;

public interface IImageFilesStorage : IFilesStorage<ImageFileModel, string>
{
}