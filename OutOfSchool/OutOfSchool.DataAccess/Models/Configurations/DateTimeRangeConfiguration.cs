using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class DateTimeRangeConfiguration : IEntityTypeConfiguration<DateTimeRange>
{
    public void Configure(EntityTypeBuilder<DateTimeRange> builder)
    {
        builder.Property(x => x.WorkshopId).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<long, DateTimeRange>();
    }
}