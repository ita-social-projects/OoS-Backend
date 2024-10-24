using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class RegionAdminConfiguration : IEntityTypeConfiguration<RegionAdmin>
{
    public void Configure(EntityTypeBuilder<RegionAdmin> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}