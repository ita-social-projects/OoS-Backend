using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services
{
    public class FilesDbContext : DbContext, IUnitOfWork
    {
        public FilesDbContext(DbContextOptions<FilesDbContext> options)
            : base(options)
        {
        }

        public DbSet<DbImageModel> Images { get; set; }

        public DbSet<DbImageContentTypeModel> ImageContentTypes { get; set; }

        public async Task<int> CompleteAsync() => await this.SaveChangesAsync();

        public int Complete() => this.SaveChanges();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DbImageModel>()
                .HasOne(x => x.ContentType)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ContentTypeId);

            builder.SeedFilesData();
        }
    }
}
