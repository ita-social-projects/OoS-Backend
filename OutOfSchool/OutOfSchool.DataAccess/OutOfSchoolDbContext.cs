using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services
{
    public class OutOfSchoolDbContext : DbContext
    {
        public OutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(b => b.MigrationsAssembly("OutOfSchool.WebApi"));
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
    }
}