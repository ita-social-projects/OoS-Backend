using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasMany(x => x.ProviderAdmins)
            .WithMany(x => x.ManagedWorkshops);
        builder.HasMany(x => x.Teachers)
            .WithOne(x => x.Workshop)
            .HasForeignKey(x => x.WorkshopId);
        builder.HasMany(x => x.WorkshopDescriptionItems)
            .WithOne(x => x.Workshop)
            .HasForeignKey(x => x.WorkshopId);
    }
}
