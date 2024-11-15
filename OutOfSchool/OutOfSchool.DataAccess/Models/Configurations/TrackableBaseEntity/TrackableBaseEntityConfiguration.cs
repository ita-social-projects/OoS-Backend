using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.BaseEntities;

namespace OutOfSchool.Services.Models.Configurations.BaseEntity;

/// <summary>
///    Abstract base configuration for all entities that require tracking metadata.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public abstract class TrackableBaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : TrackableBaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ModifiedBy)
           .HasMaxLength(36);

        builder.Property(e => e.ModifiedAt);
    }
}
