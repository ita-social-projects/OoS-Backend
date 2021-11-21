using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly OutOfSchoolDbContext dbContext;

        public ImageRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ImageMetadata> GetMetadataById(Guid id)
        {
            return await dbContext.ImagesMetadata.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
