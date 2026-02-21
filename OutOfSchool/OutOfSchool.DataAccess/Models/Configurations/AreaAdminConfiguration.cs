using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class AreaAdminConfiguration : IEntityTypeConfiguration<AreaAdmin>
{
    public void Configure(EntityTypeBuilder<AreaAdmin> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}