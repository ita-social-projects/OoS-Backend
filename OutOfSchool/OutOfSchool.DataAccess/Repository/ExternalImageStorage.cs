using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Contexts;

namespace OutOfSchool.Services.Repository
{
    public class ExternalImageStorage : IExternalImageStorage
    {
        private readonly IGridFSBucket gridFsBucket;

        public ExternalImageStorage(MongoDb db)
        {
            gridFsBucket = db.GetContext();
        }

        public async Task<Stream> GetByIdAsync(string pictureId)
        {
            return await gridFsBucket.OpenDownloadStreamAsync(new ObjectId(pictureId));
        }

        public async Task<string> UploadImageAsync(Stream contentStream, CancellationToken cancellationToken = default)
        {
            try
            {
                contentStream.Position = uint.MinValue;
                var objectId = await gridFsBucket.UploadFromStreamAsync(Guid.NewGuid().ToString(), contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);

                return objectId.ToString();
            }
            catch (Exception ex)
            {
                throw new ImageStorageException(ex);
            }
        }

        //private async Task UploadFileAsync()
        //{
        //    foreach (var file in Directory.GetFiles(@"C:\Users\provi\Desktop\testImages"))
        //    {
        //        await using Stream fs = new FileStream(file, FileMode.Open);
        //        ObjectId id = await MongoDb.GetContext().UploadFromStreamAsync(Path.GetFileName(file), fs);
        //    }
        //}
    }
}
