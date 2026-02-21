using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderSectionItemConfiguration : IEntityTypeConfiguration<ProviderSectionItem>
{
    public void Configure(EntityTypeBuilder<ProviderSectionItem> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<Guid, ProviderSectionItem>();
    }
}