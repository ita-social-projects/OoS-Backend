using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Extensions
{
    public static class ModelBuilderExtension
    {
        /// <summary>
        /// Add initial data for Social Group.
        /// </summary>
        /// <param name="builder">Model Builder.</param>
        public static void Seed(this ModelBuilder builder)
        {
            builder.Entity<SocialGroup>().HasData(
                new SocialGroup
                {
                    Id = 1,
                    Name = "Діти із багатодітних сімей",
                },
                new SocialGroup
                {
                    Id = 2,
                    Name = "Діти із малозабезпечених сімей",
                },
                new SocialGroup
                {
                    Id = 3,
                    Name = "Діти з інвалідністю",
                },
                new SocialGroup
                {
                    Id = 4,
                    Name = "Діти-сироти",
                },
                new SocialGroup
                {
                    Id = 5,
                    Name = "Діти, позбавлені батьківського піклування",
                });

            builder.Entity<InstitutionStatus>().HasData(
                new InstitutionStatus
                {
                    Id = 1,
                    Name = "Працює",
                },
                new InstitutionStatus
                {
                    Id = 2,
                    Name = "Перебуває в стані реорганізації",
                },
                new InstitutionStatus
                {
                    Id = 3,
                    Name = "Має намір на реорганізацію",
                });
        }

        /// <summary>
        /// Add configuration to reduce field sizes of Identity User and Role.
        /// </summary>
        /// <param name="builder">Model Builder.</param>
        public static void UpdateIdentityTables(this ModelBuilder builder)
        {
            builder.Entity<User>(u =>
            {
                u.Property(user => user.PhoneNumber)
                    .IsUnicode(false)
                    .IsFixedLength(false)
                    .HasMaxLength(15);

                u.Property(user => user.PasswordHash)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .HasMaxLength(84);

                u.Property(user => user.ConcurrencyStamp)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .HasMaxLength(36)
                    .IsRequired(true);

                u.Property(user => user.SecurityStamp)
                    .IsUnicode(false)
                    .IsFixedLength(false)
                    .HasMaxLength(36)
                    .IsRequired(true);
            });

            builder.Entity<IdentityRole>(r =>
            {
                r.Property(role => role.ConcurrencyStamp)
                    .IsUnicode(false)
                    .IsFixedLength(true)
                    .HasMaxLength(36)
                    .IsRequired(true);
            });
        }
    }
}