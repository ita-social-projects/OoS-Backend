using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class TechAdminConfiguration : IEntityTypeConfiguration<TechAdmin>
{
    public void Configure(EntityTypeBuilder<TechAdmin> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.Individual)
            .WithOne()
            .HasForeignKey<TechAdmin>(x => x.Id)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
