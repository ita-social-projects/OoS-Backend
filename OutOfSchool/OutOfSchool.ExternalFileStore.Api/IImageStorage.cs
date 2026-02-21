using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore;

/// <summary>
/// Represents image-specific storage operations.
/// </summary>
public interface IImageStorage : IFilesStorage<ImageFileModel, string>;