using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore;

public interface IObjectImageStorage : IObjectStorage<ImageFileModel, string>, IImageStorage;