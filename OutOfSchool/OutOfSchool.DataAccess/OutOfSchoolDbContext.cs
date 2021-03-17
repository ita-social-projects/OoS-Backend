using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services
{
    public class OutOfSchoolDbContext : IdentityDbContext<User>
    {
        public OutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<Child> Children { get; set; }

        public DbSet<Workshop> Workshops { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Subcategory> Subcategories { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<SocialGroup> SocialGroups { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}