using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Contexts.Configuration;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files
{
    public class GcpImagesStorage : GcpFilesStorageBase<ImageFileModel>, IImageFilesStorage
    {
        public GcpImagesStorage(IOptions<GcpStorageImagesSourceConfig> options)
            : base(options.Value)
        {
        }
    }
}