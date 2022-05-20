using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Contexts.Configuration;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files
{
    public abstract class GcpFilesStorageBase<TFile> : IFilesStorage<TFile, string>, IDisposable
        where TFile : FileModel, new()
    {
        protected GcpFilesStorageBase(IGcpStorageContext storageContext)
        {
            StorageClient = storageContext.StorageClient;
            BucketName = storageContext.BucketName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private protected StorageClient StorageClient { get; }

        private protected string BucketName { get; }

        /// <inheritdoc/>
        public virtual async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
            try
            {
                var fileObject = await StorageClient.GetObjectAsync(
                    BucketName,
                    fileId,
                    cancellationToken: cancellationToken);

                var fileStream = new MemoryStream();
                await StorageClient.DownloadObjectAsync(
                    fileObject,
                    fileStream,
                    cancellationToken: cancellationToken);

                fileStream.Position = 0;
                return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<string> UploadAsync(TFile file, CancellationToken cancellationToken = default)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));

            try
            {
                var fileId = GenerateFileId();
                var dataObject = await StorageClient.UploadObjectAsync(
                    BucketName,
                    fileId,
                    file.ContentType,
                    file.ContentStream,
                    cancellationToken: cancellationToken);
                return dataObject.Name;
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
            try
            {
                await StorageClient.DeleteObjectAsync(BucketName, fileId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }

        /// <summary>
        /// This method generates a unique value that is used as file identifier.
        /// </summary>
        /// <returns>
        /// The result contains a string value of the file if it's uploaded.
        /// </returns>
        protected virtual string GenerateFileId()
        {
            return Guid.NewGuid().ToString();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StorageClient?.Dispose();
            }
        }
    }
}