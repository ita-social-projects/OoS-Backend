using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderSectionItemConfiguration : IEntityTypeConfiguration<ProviderSectionItem>
{
    public void Configure(EntityTypeBuilder<ProviderSectionItem> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}