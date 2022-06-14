using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
    {
        public void Configure(EntityTypeBuilder<Achievement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.AchievementType)
                .WithMany()
                .IsRequired()
                .HasForeignKey(x => x.AchievementTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Children)
                .WithMany(x => x.Achievements);
        }
    }
}
