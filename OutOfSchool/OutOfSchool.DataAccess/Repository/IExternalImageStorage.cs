﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository
{
    public interface IExternalImageStorage
    {
        Task<ExternalImageModel> GetByIdAsync(string imageId);

        Task<string> UploadImageAsync(ExternalImageModel imageModel, CancellationToken cancellationToken = default);

        Task DeleteImageAsync(string imageId, CancellationToken cancellationToken = default);
    }
}
