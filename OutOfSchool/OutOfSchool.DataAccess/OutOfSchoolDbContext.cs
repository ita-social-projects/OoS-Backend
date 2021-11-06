using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DbSet<InstitutionStatus> InstitutionStatuses { get; set; }

        public DbSet<PermissionsForRole> PermissionsForRoles { get; set; }

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

            builder.Entity<Provider>()
                .HasMany(pa => pa.ProviderAdmins)
                .WithOne(p => p.Provider)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<City>()
                .HasMany(pa => pa.ProviderAdmins)
                .WithOne(c => c.City)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProviderAdmin>()
                .HasOne(pa => pa.User)
                .WithOne(u => u.ProviderAdmin)
                .HasForeignKey<ProviderAdmin>(pa => pa.UserId)
                .OnDelete(DeleteBehavior.NoAction);

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