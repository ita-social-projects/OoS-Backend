using System.Threading.Tasks;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Configurations;

namespace OutOfSchool.Services
{
    public class OutOfSchoolDbContext : IdentityDbContext<User>, IDataProtectionKeyContext, IUnitOfWork
    {
        public OutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<Parent> Parents { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<ChatRoom> ChatRooms { get; set; }

        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Child> Children { get; set; }

        public DbSet<Workshop> Workshops { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Class> Classes { get; set; }

        public DbSet<Direction> Directions { get; set; }

        public DbSet<SocialGroup> SocialGroups { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<DateTimeRange> DateTimeRanges { get; set; }

        public async Task<int> CompleteAsync() => await this.SaveChangesAsync();

        public int Complete() => this.SaveChanges();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DateTimeRange>()
                .HasCheckConstraint("CK_DateTimeRanges_EndTimeIsAfterStartTime", "[EndTime] >= [StartTime]");

            builder.ApplyConfiguration(new TeacherConfiguration());
            builder.ApplyConfiguration(new ApplicationConfiguration());
            builder.ApplyConfiguration(new ChildConfiguration());
            builder.ApplyConfiguration(new ChatMessageConfiguration());
            builder.ApplyConfiguration(new ChatRoomConfiguration());
            builder.ApplyConfiguration(new ChatRoomUserConfiguration());
            builder.ApplyConfiguration(new ProviderConfiguration());
            builder.ApplyConfiguration(new WorkshopConfiguration());

            builder.Seed();
            builder.UpdateIdentityTables();
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}