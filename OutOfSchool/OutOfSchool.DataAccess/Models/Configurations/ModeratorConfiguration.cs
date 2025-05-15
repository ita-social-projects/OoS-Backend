using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ModeratorConfiguration : IEntityTypeConfiguration<Moderator>
{
    public void Configure(EntityTypeBuilder<Moderator> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.Individual)
            .WithOne()
            .HasForeignKey<Moderator>(x => x.Id)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}