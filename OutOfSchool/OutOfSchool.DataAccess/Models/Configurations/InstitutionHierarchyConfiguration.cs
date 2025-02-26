using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models.Configurations;

internal class InstitutionHierarchyConfiguration : IEntityTypeConfiguration<InstitutionHierarchy>
{
    public void Configure(EntityTypeBuilder<InstitutionHierarchy> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<Guid, InstitutionHierarchy>();
        
        // This is needed only for external system sync at the moment
        // It relies on MySQL default value on update feature
        // If we require this for our own logic - need to refactor to interceptor.
        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("NULL ON UPDATE CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnUpdate();
    }
}
