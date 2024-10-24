using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models.Configurations;

internal class InstitutionFieldDescriptionConfiguration : IEntityTypeConfiguration<InstitutionFieldDescription>
{
    public void Configure(EntityTypeBuilder<InstitutionFieldDescription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}
