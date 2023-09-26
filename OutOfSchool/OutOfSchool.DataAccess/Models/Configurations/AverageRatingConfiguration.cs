
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class AverageRatingConfiguration : IEntityTypeConfiguration<AverageRating>
{
    public void Configure(EntityTypeBuilder<AverageRating> builder)
    {
        builder.HasIndex(x => x.EntityId);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
