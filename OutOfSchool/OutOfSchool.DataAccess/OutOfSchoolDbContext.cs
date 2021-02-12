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


        public DbSet<Parent> Parents { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<DirectionOfEducation> DirectionsOfEducation { get; set; }
        public DbSet<ProfileOfEducation> ProfilesOfEducation { get; set; }
    }
}