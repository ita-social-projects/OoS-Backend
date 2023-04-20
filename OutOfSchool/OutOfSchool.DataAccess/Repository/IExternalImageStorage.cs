using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository;

public interface IExternalImageStorage
{
    Task<ImageFileModel> GetByIdAsync(string imageId);

    Task<string> UploadImageAsync(ImageFileModel imageFileModel, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageId, CancellationToken cancellationToken = default);
}