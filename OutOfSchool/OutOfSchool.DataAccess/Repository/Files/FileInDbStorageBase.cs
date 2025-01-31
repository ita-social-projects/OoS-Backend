using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api.Files;

namespace OutOfSchool.Services.Repository.Files;

public abstract class FileInDbStorageBase<TFile> : IFilesStorage<TFile, string>
    where TFile : FileModel, new()
{
    private readonly IFileInDbRepository fileInDbRepository;

    protected FileInDbStorageBase(
        IFileInDbRepository fileInDbRepository)
    {
        this.fileInDbRepository = fileInDbRepository;
    }

    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var fileInDb = await fileInDbRepository.GetById(fileId).ConfigureAwait(false);

        if (fileInDb != null)
        {
            await fileInDbRepository.Delete(fileInDb).ConfigureAwait(false);
        }
    }

    public async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

        try
        {
            var fileInDb = await fileInDbRepository.GetById(fileId).ConfigureAwait(false);

            var memoryStream = new MemoryStream();
            memoryStream.Write(fileInDb.Data, 0, fileInDb.Data.Length);
            memoryStream.Position = 0;
            return new TFile { ContentStream = memoryStream, ContentType = fileInDb.ContentType };
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Note: The cacheControl and metadata parameters are not used in the database storage implementation
    /// as they are primarily intended for cloud storage scenarios.
    /// </remarks>
    public async Task<string> UploadAsync(TFile file, string cacheControl, IDictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));

        var fileInDb = new FileInDb()
        {
            Id = GenerateFileId(),
            ContentType = file.ContentType,
            Data = ((MemoryStream)file.ContentStream).ToArray(),
        };

        await fileInDbRepository.Create(fileInDb).ConfigureAwait(false);

        return fileInDb.Id;
    }

    /// <inheritdoc/>
    public string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}
