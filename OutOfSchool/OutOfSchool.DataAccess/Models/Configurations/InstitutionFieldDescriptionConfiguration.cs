using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models.Configurations;

internal class InstitutionFieldDescriptionConfiguration : IEntityTypeConfiguration<InstitutionFieldDescription>
{
    public void Configure(EntityTypeBuilder<InstitutionFieldDescription> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<Guid, InstitutionFieldDescription>();
    }
}
