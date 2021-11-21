using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.Pictures;

namespace OutOfSchool.Services.Repository
{
    public class PictureRepository : IPictureRepository
    {
        private readonly OutOfSchoolDbContext dbContext;

        public PictureRepository(OutOfSchoolDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PictureMetadata> GetMetadataById(Guid id)
        {
            return await dbContext.PicturesMetadata.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
