using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.Property(x => x.EntityId).HasColumnType("UUID");
        builder.HasIndex(x => x.EntityId);

        builder.ConfigureKeyedSoftDeleted<long, Rating>();
    }
}
