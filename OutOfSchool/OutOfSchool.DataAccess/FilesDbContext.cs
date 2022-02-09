using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services
{
    public class FilesDbContext : DbContext
    {
        public FilesDbContext(DbContextOptions<FilesDbContext> options)
            : base(options)
        {
        }

        public DbSet<DbImageModel> Images { get; set; }
    }
}
