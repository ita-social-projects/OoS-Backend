
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class AverageRatingConfiguration : IEntityTypeConfiguration<AverageRating>
{
    public void Configure(EntityTypeBuilder<AverageRating> builder)
    {
        builder.HasIndex(x => x.EntityId);
        builder.Property<bool>("IsDeleted").HasDefaultValue(false);
    }
}
