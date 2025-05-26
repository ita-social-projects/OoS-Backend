using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ModeratorConfiguration : IEntityTypeConfiguration<Moderator>
{
    /// <summary>
    /// Configures the entity mapping for the <see cref="Moderator"/> model, including property defaults, indexing, and relationships.
    /// </summary>
    /// <param name="builder">The builder used to configure the <see cref="Moderator"/> entity.</param>
    public void Configure(EntityTypeBuilder<Moderator> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.Individual)
            .WithOne()
            .HasForeignKey<Moderator>(x => x.Id)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}