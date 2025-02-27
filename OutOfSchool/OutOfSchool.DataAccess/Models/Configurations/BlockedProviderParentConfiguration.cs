using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class BlockedProviderParentConfiguration : IEntityTypeConfiguration<BlockedProviderParent>
{
    public void Configure(EntityTypeBuilder<BlockedProviderParent> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<Guid, BlockedProviderParent>();
    }
}
