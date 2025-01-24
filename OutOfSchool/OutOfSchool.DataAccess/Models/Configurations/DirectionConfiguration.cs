using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class DirectionConfiguration : IEntityTypeConfiguration<Direction>
{
    public void Configure(EntityTypeBuilder<Direction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        // This is needed only for external system sync at the moment
        // It relies on MySQL default value on update feature
        // If we require this for our own logic - need to refactor to interceptor.
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("NULL ON UPDATE CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnUpdate();
    }
}