using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Extensions;
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

        public DbSet<ChatRoom> ChatRooms { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Child> Children { get; set; }

        public DbSet<Workshop> Workshops { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Class> Classes { get; set; }

        public DbSet<Direction> Directions { get; set; }

        public DbSet<SocialGroup> SocialGroups { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<BirthCertificate> BirthCertificates { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.ChatRoom)
                .WithMany(r => r.ChatMessages)
                .HasForeignKey(r => r.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoom>()
                .HasOne(r => r.Parent)
                .WithMany(p => p.ChatRooms)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoom>()
                .HasOne(r => r.Workshop)
                .WithMany(w => w.ChatRooms)
                .HasForeignKey(r => r.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Provider>()
                .HasKey(x => x.Id);

            builder.Entity<Provider>()
                .HasOne(x => x.User);

            builder.Entity<Provider>()
                .HasMany(x => x.Workshops)
                .WithOne(w => w.Provider);

            builder.Entity<Provider>()
                .HasOne(x => x.LegalAddress)
                .WithMany()
                .HasForeignKey(x => x.LegalAddressId)
                .IsRequired();

            builder.Entity<Provider>()
                .HasOne(x => x.ActualAddress)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasForeignKey(x => x.ActualAddressId)
                .IsRequired(false);

            builder.Seed();
            builder.UpdateIdentityTables();
        }
    }
}