using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations.Base;

public abstract class BusinessEntityConfiguration<TBase> : IEntityTypeConfiguration<TBase>
    where TBase : BusinessEntity
{
    public virtual void Configure(EntityTypeBuilder<TBase> entityTypeBuilder)
    {
        entityTypeBuilder.HasKey(x => x.Id);

        entityTypeBuilder.HasIndex(x => x.IsDeleted);

        entityTypeBuilder.Property(x => x.IsDeleted).HasDefaultValue(false);

        entityTypeBuilder.Property(e => e.CreatedBy)
            .HasField("_createdBy");

        entityTypeBuilder.Property(e => e.ModifiedBy)
            .HasField("_modifiedBy");

        entityTypeBuilder.Property(e => e.DeletedBy)
            .HasField("_deletedBy");

        entityTypeBuilder.Property(e => e.CreatedAt)
            .HasField("_createdAt");

        entityTypeBuilder.Property(e => e.UpdatedAt)
            .HasField("_updatedAt");

        entityTypeBuilder.Property(e => e.DeleteDate)
            .HasField("_deleteDate");

        entityTypeBuilder.Property(e => e.IsSystemProtected)
            .HasField("_isSystemProtected");

        entityTypeBuilder.Property(e => e.ActiveTo)
            .HasDefaultValue(DateOnly.MaxValue);
    }
}