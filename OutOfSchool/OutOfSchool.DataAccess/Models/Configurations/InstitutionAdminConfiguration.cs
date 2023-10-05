using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class InstitutionAdminConfiguration : IEntityTypeConfiguration<InstitutionAdmin>
{
    public void Configure(EntityTypeBuilder<InstitutionAdmin> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}