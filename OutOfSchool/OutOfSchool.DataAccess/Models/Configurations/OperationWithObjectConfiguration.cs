using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class OperationWithObjectConfiguration : IEntityTypeConfiguration<OperationWithObject>
{
    public void Configure(EntityTypeBuilder<OperationWithObject> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EntityId).HasColumnType("UUID");

        builder.HasIndex(x => x.OperationType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.RowSeparator);
    }
}
