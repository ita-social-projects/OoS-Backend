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
    public class ExternalImageStorage : IExternalImageStorage
    {
        private const string ContentType = "ContentType";

        private readonly IGridFSBucket gridFsBucket;

        public ExternalImageStorage(MongoDb db)
        {
            gridFsBucket = db.GetContext();
        }

        public async Task<ExternalImageModel> GetByIdAsync(string imageId)
        {
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));
            try
            {
                var result = await gridFsBucket.OpenDownloadStreamAsync(new ObjectId(imageId))
                    ?? throw new InvalidOperationException($"Unreal to get non-nullable {nameof(GridFSDownloadStream)} instance."); // think about searching by file name
                var contentType = result.FileInfo.Metadata[ContentType].AsString;
                return new ExternalImageModel { ContentStream = result, ContentType = contentType };
            }
            catch (Exception ex)
            {
                throw new ImageStorageException(ex);
            }
        }

        public async Task<string> UploadImageAsync(ExternalImageModel imageModel, CancellationToken cancellationToken = default)
        {
            _ = imageModel ?? throw new ArgumentNullException(nameof(imageModel));
            try
            {
                imageModel.ContentStream.Position = uint.MinValue;
                var options = new GridFSUploadOptions
                {
                    Metadata = new BsonDocument(ContentType, imageModel.ContentType),
                };
                var objectId = await gridFsBucket.UploadFromStreamAsync(Guid.NewGuid().ToString(), imageModel.ContentStream, options, cancellationToken: cancellationToken).ConfigureAwait(false);

                return objectId.ToString();
            }
            catch (Exception ex)
            {
                throw new ImageStorageException(ex);
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
                throw new ImageStorageException(ex);
            }
        }
    }
}
