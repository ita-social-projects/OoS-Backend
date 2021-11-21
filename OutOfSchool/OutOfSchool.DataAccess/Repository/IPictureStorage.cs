using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public interface IPictureStorage
    {
        Task<Stream> GetByIdAsync(string pictureId);

        public Task<string> UploadPictureAsync(Stream contentStream, CancellationToken cancellationToken = default);
    }
}
