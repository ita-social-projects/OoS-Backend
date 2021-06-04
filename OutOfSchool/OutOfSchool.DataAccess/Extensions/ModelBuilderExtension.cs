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
        }
    }
}
