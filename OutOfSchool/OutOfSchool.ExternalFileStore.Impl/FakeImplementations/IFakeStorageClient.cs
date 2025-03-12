using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeImplementations;

/// <summary>
/// Only for development purposes. Used as fake storage client interface whenever no need to interplay with storage.
/// </summary>
public interface IFakeStorageClient
{
    Task DeleteAsync(string fileId) => Task.CompletedTask;
    Task<FileModel> GetByIdAsync(string fileId) => Task.FromResult(new FileModel());
}
