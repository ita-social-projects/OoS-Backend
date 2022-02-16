using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<int> CompleteAsync() => await this.SaveChangesAsync();

        public int Complete() => this.SaveChanges();
    }
}
