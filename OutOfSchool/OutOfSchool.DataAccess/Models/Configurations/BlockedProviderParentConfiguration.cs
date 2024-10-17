using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class BlockedProviderParentConfiguration : IEntityTypeConfiguration<BlockedProviderParent>
{
    public void Configure(EntityTypeBuilder<BlockedProviderParent> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
