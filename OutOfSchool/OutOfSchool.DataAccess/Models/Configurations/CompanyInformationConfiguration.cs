using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompanyInformationConfiguration : IEntityTypeConfiguration<CompanyInformation>
{
    public void Configure(EntityTypeBuilder<CompanyInformation> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}