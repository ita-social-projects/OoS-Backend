using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class TechAdminConfiguration : IEntityTypeConfiguration<TechAdmin>
{
    /// <summary>
    /// Configures the entity mapping for the <c>TechAdmin</c> type, including property defaults, indexing, and relationships.
    /// </summary>
    /// <param name="builder">The builder used to configure the <c>TechAdmin</c> entity.</param>
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
