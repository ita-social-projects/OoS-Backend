using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository
{
    // TODO: think about disposing streams, contexts in future
    public class ExternalImageStorage : IExternalImageStorage
    {
        private const string ContentType = "ContentType";

        private readonly IGridFSBucket gridFsBucket;

        public ExternalImageStorage(MongoDb db)
        {
            gridFsBucket = db.GetContext();
        }

        public async Task<ImageFileModel> GetByIdAsync(string imageId)
        {
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));
            try
            {
                var result = await gridFsBucket.OpenDownloadStreamAsync(new ObjectId(imageId))
                             ?? throw new InvalidOperationException($"Unreal to get non-nullable {nameof(GridFSDownloadStream)} instance."); // think about searching by file name
                var contentType = result.FileInfo.Metadata[ContentType].AsString;
                return new ImageFileModel { ContentStream = result, ContentType = contentType };
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }

        public async Task<string> UploadImageAsync(ImageFileModel imageFileModel, CancellationToken cancellationToken = default)
        {
            _ = imageFileModel ?? throw new ArgumentNullException(nameof(imageFileModel));
            try
            {
                imageFileModel.ContentStream.Position = uint.MinValue;
                var options = new GridFSUploadOptions
                {
                    Metadata = new BsonDocument(ContentType, imageFileModel.ContentType),
                };
                var objectId = await gridFsBucket.UploadFromStreamAsync(Guid.NewGuid().ToString(), imageFileModel.ContentStream, options, cancellationToken: cancellationToken).ConfigureAwait(false);

                return objectId.ToString();
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }

        public async Task DeleteImageAsync(string imageId, CancellationToken cancellationToken = default)
        {
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));
            try
            {
                await gridFsBucket.DeleteAsync(new ObjectId(imageId), cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FileStorageException(ex);
            }
        }
    }
}
