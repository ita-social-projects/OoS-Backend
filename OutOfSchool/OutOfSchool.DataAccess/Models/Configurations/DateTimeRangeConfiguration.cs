using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class DateTimeRangeConfiguration : IEntityTypeConfiguration<DateTimeRange>
{
    public void Configure(EntityTypeBuilder<DateTimeRange> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}