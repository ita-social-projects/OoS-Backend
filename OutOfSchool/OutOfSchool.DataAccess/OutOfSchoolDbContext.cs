using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services
{
    public class OutOfSchoolDbContext : IdentityDbContext<User>
    {
        public OutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost,1433; Database=Master;User Id=SA;Password=Oos-password1",b =>
            {
                b.MigrationsAssembly("OutOfSchool.IdentityServer");
            });
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Child> Children { get; set; }
    }
}