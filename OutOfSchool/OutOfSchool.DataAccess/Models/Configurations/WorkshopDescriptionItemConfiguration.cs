using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class WorkshopDescriptionItemConfiguration : IEntityTypeConfiguration<WorkshopDescriptionItem>
{
    public void Configure(EntityTypeBuilder<WorkshopDescriptionItem> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}