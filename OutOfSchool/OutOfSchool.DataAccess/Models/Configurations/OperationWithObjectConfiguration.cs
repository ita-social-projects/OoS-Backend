using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class OperationWithObjectConfiguration : IEntityTypeConfiguration<OperationWithObject>
{
    public void Configure(EntityTypeBuilder<OperationWithObject> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.OperationType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.RowSeparator);
    }
}
