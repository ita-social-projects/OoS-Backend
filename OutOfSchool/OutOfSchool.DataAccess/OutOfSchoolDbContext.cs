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

        public DbSet<Child> Children { get; set; }

        public DbSet<Workshop> Workshops { get; set; }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<Subcategory> Subcategories { get; set; }

        public DbSet<Subsubcategory> Subsubcategories { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<SocialGroup> SocialGroups { get; set; }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<BirthCertificate> BirthCertificates { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Rating> Ratings { get; set; }

        public DbSet<ChatRoom> ChatRooms { get; set; }

        public DbSet<ChatRoomUser> ChatRoomUsers { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.Seed();
        }
    }
}