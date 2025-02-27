using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

public class ChangesLogConfiguration: IEntityTypeConfiguration<ChangesLog>
{
    public void Configure(EntityTypeBuilder<ChangesLog> builder)
    {
        builder.Property(x => x.EntityIdGuid).HasColumnType("UUID");
    }
}