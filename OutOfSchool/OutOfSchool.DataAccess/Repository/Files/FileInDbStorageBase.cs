using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files;

public abstract class FileInDbStorageBase<TFile> : IFilesStorage<TFile, string>
    where TFile : FileModel, new()
{
    private readonly IFileInDbRepository fileInDbRepository;

    public FileInDbStorageBase(
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

    public IAsyncEnumerable<Objects> GetBulkListsOfObjectsAsync(string prefix = null, ListObjectsOptions options = null)
    {
        throw new NotImplementedException();
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

    public async Task<string> UploadAsync(TFile file, CancellationToken cancellationToken = default)
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

    protected virtual string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}
