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

        public DbSet<BirthCertificate> BirthCertificates { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<WorkshopPicture> WorkshopPictureTable { get; set; }

        public DbSet<ProviderPicture> ProviderPictureTable { get; set; }

        public DbSet<TeacherPicture> TeacherPictureTable { get; set; }

        public DbSet<PictureMetadata> PictureMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<WorkshopPicture>().HasKey(nameof(WorkshopPicture.WorkshopId), nameof(WorkshopPicture.PictureId));

            builder.Entity<WorkshopPicture>()
                .HasOne(x => x.Workshop)
                .WithMany(x => x.WorkshopPictures)
                .HasForeignKey(x => x.WorkshopId);

            builder.Entity<WorkshopPicture>()
                .HasOne(x => x.Picture)
                .WithOne(x => x.WorkshopPicture);

            builder.Entity<ProviderPicture>().HasKey(nameof(ProviderPicture.ProviderId), nameof(ProviderPicture.PictureId));

            builder.Entity<ProviderPicture>()
                .HasOne(x => x.Provider)
                .WithMany(x => x.ProviderPictures)
                .HasForeignKey(x => x.ProviderId);

            builder.Entity<ProviderPicture>()
                .HasOne(x => x.Picture)
                .WithOne(x => x.ProviderPicture);

            builder.Entity<TeacherPicture>().HasKey(nameof(TeacherPicture.TeacherId), nameof(TeacherPicture.PictureId));

            builder.Entity<TeacherPicture>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.TeacherPictures)
                .HasForeignKey(x => x.TeacherId);

            builder.Entity<TeacherPicture>()
                .HasOne(x => x.Picture)
                .WithOne(x => x.TeacherPicture);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.ChatRoom)
                .WithMany(r => r.ChatMessages)
                .HasForeignKey(r => r.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoom>()
                .HasMany(r => r.Users)
                .WithMany(u => u.ChatRooms)
                .UsingEntity<ChatRoomUser>(
                j => j
                    .HasOne(cru => cru.User)
                    .WithMany(u => u.ChatRoomUsers)
                    .HasForeignKey(cru => cru.UserId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(cru => cru.ChatRoom)
                    .WithMany(r => r.ChatRoomUsers)
                    .HasForeignKey(cru => cru.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade));

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