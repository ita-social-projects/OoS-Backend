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
    }
}
