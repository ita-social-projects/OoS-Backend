using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository
{
    public class ExternalMysqlImageStorage : IExternalImageStorage
    {
        private readonly FilesDbContext dbContext;

        public ExternalMysqlImageStorage(FilesDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ExternalImageModel> GetByIdAsync(string imageId)
        {
            _ = imageId ?? throw new ArgumentNullException(nameof(imageId));
            try
            {
                var searchResult = await dbContext.Images.FirstAsync(x => x.Id == new Guid(imageId)).ConfigureAwait(false);
                return new ExternalImageModel
                {
                    ContentStream = new MemoryStream(searchResult.File),
                    ContentType = searchResult.ContentType,
                };
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

                await using var fileStream = new MemoryStream();
                await imageModel.ContentStream.CopyToAsync(fileStream, cancellationToken);

                var newImage = new DbImageModel
                {
                    Id = default,
                    ContentType = imageModel.ContentType,
                    File = fileStream.ToArray(),
                };

                await dbContext.Images.AddAsync(newImage, cancellationToken).ConfigureAwait(false);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return newImage.Id.ToString();
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
                dbContext.Images.Remove(new DbImageModel { Id = new Guid(imageId) });
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ImageStorageException(ex);
            }
        }
    }
}
