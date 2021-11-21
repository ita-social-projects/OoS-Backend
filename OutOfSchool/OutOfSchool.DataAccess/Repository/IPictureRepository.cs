using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.Pictures;

namespace OutOfSchool.Services.Repository
{
    public interface IPictureRepository
    {
        public Task<PictureMetadata> GetMetadataById(Guid id);
    }
}
