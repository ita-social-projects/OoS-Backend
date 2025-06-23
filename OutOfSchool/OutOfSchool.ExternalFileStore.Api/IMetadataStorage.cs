namespace OutOfSchool.ExternalFileStore;
public interface IMetadataStorage
{
    Task<IDictionary<string, string>> GetCurrentMetadataAsync(string objectId, CancellationToken cancellationToken = default);
    Task UpdateMetadataAsync(string objectId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
}